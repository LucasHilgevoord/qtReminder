using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using qtReminder.AnimeReminder.Models;
using qtReminder.AnimeReminder.Nyaa;

namespace qtReminder.AnimeReminder.Services
{
    /// <summary>
    /// Handles the checking of the anime.
    /// </summary>
    public static class AnimeReminderHandler
    {
        private static bool _checkingStarted = false;
        
        // The time how long the checker will wait before checking Nyaa.si
        private static double _waitTime = 5.0;

        // The min and max wait time. 
        // When the checker finds someting, it sets the wait time to the lowest value.
        // Then everytime it doesn't find something again, it sets that value back up by increments of
        // WaitTimeIncrement. This will hopefully cause minimal overhead for the RSS server
        // But still provides fast updates in times that it needs it.
        private const double MaxWaitTime = 5.0;
        private const double MinWaitTime = 0.5;
        private const double WaitTimeIncrement = 0.5;
        
        public static void StartCheck()
        {
            if (_checkingStarted) return;

            Task.Factory.StartNew(async () => await WaitCheck())
                .ConfigureAwait(false);

            _checkingStarted = true;
        }

        private static async Task WaitCheck()
        {
            while (true)
            {
                try
                {
                    bool result = await Check();

                    if (result) 
                        _waitTime = MinWaitTime;
                    else if (_waitTime > MaxWaitTime) 
                        _waitTime = Math.Min(MaxWaitTime, _waitTime + WaitTimeIncrement);                
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
                await Task.Delay(TimeSpan.FromMinutes(_waitTime));
            }
            // ReSharper disable once FunctionNeverReturns FIRE AND FORGET METHOD.
        }
        
        /// <summary>
        /// Performs the actual check for the anime
        /// </summary>
        /// <returns>true if something has been found</returns>
        public static async Task<bool> Check()
        {
            // get the XML, and parse it
            var xml = await NyaaRSSFeed.GetRSSFeed();
            var parsedXML = NyaaXMLConverter.ParseXML(xml);
            NyaaTorrentModel[] rawTorrents = NyaaXMLConverter.GetTorrents(parsedXML);
            ParsedNyaaTorrentModel[] parsedTorrents = NyaaParser.ParseNyaaTorrents(rawTorrents);
            
            string lastChecked = Database.Database.GetLastChecked();
            
            // get ALL EPIC ANIME
            var (_, collection) = Database.Database.GetDatabaseAndSubscriptionCollection();

            var guildAnimes = collection.FindAll().ToList();
            var animeToAnnounce = new List<AnnounceModel>();

            bool foundSomething = false;
            
            foreach (var parsedTorrent in parsedTorrents)
            {
                // if the title is correct, add it to the update list.
                var validAnime = guildAnimes.Where(x =>
                {
                    bool valid = true;

                    const double minSim = 0.55;
                    
                    double confidence = Math.Max(
                        (x.AnimeTitle.EnglishTitle ?? "").ToLower().GetSimilarity((parsedTorrent.AnimeTitle ?? "").ToLower()),
                        x.AnimeTitle.RomajiTitle.ToLower().GetSimilarity((parsedTorrent.AnimeTitle ?? "").ToLower()));
                    
                    valid = confidence >= minSim;

                    valid = valid && x.WantedSubgroupTitle.Contains(parsedTorrent.SubGroup.ToLower());

                    valid = valid && parsedTorrent.Episode >= x.LastAnnouncedEpisode;
                    
                    return valid;
                });

                foreach (var anime in validAnime)
                {
                    var m = new AnnounceModel()
                    {
                        AnimeGuildModel = anime,
                        Episode = parsedTorrent.Episode
                    };
                    
                    if(parsedTorrent.Quality != Quality.Unknown)
                        m.QualityLinks[parsedTorrent.Quality] = new SubgroupTorrentLink(parsedTorrent.NyaaTorrentModel.Link,
                            parsedTorrent.SubGroup);

                    if(!animeToAnnounce.Any(x => x.AnimeGuildModel.AnimeID == anime.AnimeID))
                        animeToAnnounce.Add(m);
                    AnimeReminderAnnouncer.UpdateAnime(m);

                    foundSomething = true;
                }
            }

            foreach (var anime in animeToAnnounce)
            {
                AnimeReminderAnnouncer.Announce(anime);
                anime.AnimeGuildModel.LastAnnouncedEpisode = anime.Episode;
                collection.Update(anime.AnimeGuildModel);

            }

            return foundSomething;
        }
    }
}