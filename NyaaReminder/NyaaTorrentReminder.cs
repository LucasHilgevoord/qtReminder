using System;
using System.Collections.Generic;
using Discord;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Net.Http;
using Discord.WebSocket;
using qtReminder.Models;

namespace qtReminder.Nyaa
{
    public partial class TorrentReminder
    {
        private const string OPTIONS_FILENAME = "nyaaoptions.json";
        
        private DiscordSocketClient Client { get; }
        private ReminderOptions ReminderOptions { get; set; }

        public TorrentReminder(DiscordSocketClient client)
        {
            Client = client;
            ReminderOptions = TorrentReminderOptions.LoadReminders(OPTIONS_FILENAME);
        }
        
        public async Task RepeatCheck()
        {
            while (true)
            {
                try
                {
                    const int CheckEvery_Minute = 2;

                    if (ReminderOptions.SubscribedAnime.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    Console.WriteLine($"{StringHelper.GetDateTimeString()} Checking anime ...");
                    var xml = await GetNyaaRSSAsXML();
                    var recentAnime = GetRecentNyaaAnime(xml);

                    if (recentAnime.Count != 0)
                    {
                        var animeChannels = ParseAnimeChannels(recentAnime);

                        if (animeChannels.Count != 0)
                        {
                            Console.WriteLine(
                                $"{StringHelper.GetDateTimeString()} New anime in {animeChannels.Count} channels.");
                            animeChannels.ForEach(x => x.AnimeChannel.NotifyUsers(Client, x.ParsedAnime.Episode,
                                x.AnimeChannel.CurrentAnimeTorrent, x.ParsedAnime));
                        }
                    }

                    TorrentReminderOptions.SaveReminders(OPTIONS_FILENAME, ReminderOptions, true);

                    await Task.Delay(TimeSpan.FromMinutes(CheckEvery_Minute));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"oh no :{e.Message}");
                }
            }
        }

        /// <summary>
        /// Checks if there are any relevant anime in the list that people are subscribed to.
        /// </summary>
        /// <param name="list"></param>
        /// <returns>New episodes of people that are subscribed.</returns>
        private List<ParsedAnimeChannel> ParseAnimeChannels(List<NyaaAnime> list)
        {
            var animeList = new List<ParsedAnimeChannel>();

            foreach (var nAnime in list)
            {
                var parsedTitle = ParseTitle(nAnime.Title);
                var anime = ReminderOptions.SubscribedAnime.
                    FirstOrDefault(x=>parsedTitle.Title.ToLower().Contains(x.Anime.Name.ToLower()));

                if (anime == null || 
                    anime.LatestEpisode >= parsedTitle.Episode || 
                    anime.Anime.MinQuality != parsedTitle.Quality || 
                    parsedTitle.Fangroup.ToLower() != anime.Anime.Subgroup.ToLower()) continue;


                anime.CurrentAnimeTorrent = nAnime;
                animeList.Add(new ParsedAnimeChannel(parsedTitle, anime));
            }

            return animeList;
        }

        /// <summary>
        /// Parse the XML title.
        /// </summary>
        /// <param name="title">title of the torrent.</param>
        /// <returns>cool parsed shit</returns>
        private ParsedAnime ParseTitle(string title)
        {
            string anime_title, fangroup;
            Quality quality = Quality.Unknown;
            
            var fangroupAndQuality = Regex.Match(title, "\\[(.*?)\\]");
            
            fangroup = fangroupAndQuality.Groups[1].Value;
            int titleStart = fangroupAndQuality.Index + fangroupAndQuality.Length, titleEnd;
            
            string qualitystring = fangroupAndQuality.NextMatch().Groups[1].Value;

            var episodeResultCollection = Regex.Matches(title, "([^-]+$)", RegexOptions.Multiline);
            var episodeResult = episodeResultCollection[episodeResultCollection.Count - 1];

            titleEnd = episodeResult.Index - 2; // - 3 because the title also include " - ", and that needs to go.
            //if (titleEnd < 0) titleEnd = titleStart * 2  + 1;
            string episodeString = episodeResult.Value.Trim().Split(" ")[0];
            
            if (!int.TryParse(episodeString, out int episode)) episode = -1;

            int length = titleEnd - titleStart;
            if (length < 0) length = 1;
            if (title.Length <= titleStart + length) return new ParsedAnime("", -1, "", Quality.Unknown);
            anime_title = title.Substring(titleStart, length).Trim();
            
            
            // Sorry...
            switch (qualitystring.ToLower())
            {
                case "1080p":
                    quality = Quality.x1080;
                    break;
                case "720p":
                    quality = Quality.x720;
                    break;  
                case "480p":
                    quality = Quality.x480;
                    break;
            }
            
            return new ParsedAnime(anime_title,episode,fangroup,quality);        
        }

        /// <summary>
        /// Put the XML item in a NyaaAnime object, with cool fucking information.
        /// </summary>
        /// <param name="doc">what</param>
        /// <returns></returns>
        private List<NyaaAnime> GetRecentNyaaAnime(XmlDocument doc)
        {
            var list = new List<NyaaAnime>();
            var channel = doc["rss"]["channel"];
            var childNodes = channel.ChildNodes;
            bool @checked = false;
            for (int i = 0; i < childNodes.Count; i++)
            {
                var node = childNodes.Item(i);
                if (node.Name != "item") continue;

                string infoHash = node["nyaa:infoHash"].InnerText;

                if (infoHash == ReminderOptions.LatestChecked)
                {
                    if(i == 0)
                        Console.WriteLine("Checked this one already, skipping!!");
                    return list;
                }

                if (!@checked)
                {
                    //ReminderOptions.LatestChecked = infoHash;
                    @checked = true;
                }

                string name = node["title"].InnerText;
                string link = node["guid"].InnerText;
                string s_seeders = node["nyaa:seeders"].InnerText;
                string s_leechers = node["nyaa:leechers"].InnerText;
                string size = node["nyaa:size"].InnerText;
                int seeders, leechers;
                seeders = int.TryParse(s_seeders, out seeders) ? seeders : -1;
                leechers = int.TryParse(s_seeders, out leechers) ? leechers : -1;
                
                list.Add(new NyaaAnime()
                {
                    Title = name,
                    Link = link,
                    Seeders = seeders,
                    Leechers = leechers,
                    Size = size
                });
            }

            return list;
        }

        /// <summary>
        /// Literally does what the name of it says.
        /// Gets the RSS feed of Nyaa.si (btw is this like the weeaboo nazi?)
        /// </summary>
        /// <returns></returns>
        private async Task<XmlDocument> GetNyaaRSSAsXML()
        {
            using (var hClient = new HttpClient())
            {
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, "https://nyaa.si/?page=rss");
                var asd = await hClient.SendAsync(message);
                var xml = await asd.Content.ReadAsStringAsync();

                if (!System.IO.File.Exists("garbage.xml"))
                {
                    using (var xs = System.IO.File.CreateText("garbage.xml"))
                    {
                        xs.Write(xml);
                    }
                }
                
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.LoadXml(xml);
                }
                catch (XmlException ex)
                {
                    Console.WriteLine($"Could not load XML: {ex.Message}");
                    return null;
                }
                return xmlDoc;
            }
        }
    }

    
}