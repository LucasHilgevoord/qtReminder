using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Discord.WebSocket;
using qtReminder.Models;

namespace qtReminder.Nyaa
{
    public partial class TorrentReminder
    {
        private const string OPTIONS_FILENAME = "nyaaoptions.json";
        private readonly double checkingSpeed = 2;

        public TorrentReminder(DiscordSocketClient client)
        {
            Client = client;
            ReminderOptions = TorrentReminderOptions.LoadReminders(OPTIONS_FILENAME);
        }

        private DiscordSocketClient Client { get; }
        private ReminderOptions ReminderOptions { get; }

        public async Task RepeatCheck()
        {
            while (true)
                try
                {
                    if (ReminderOptions.SubscribedAnime.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    var xml = await GetNyaaRSSAsXML();
                    var recentAnime = GetRecentNyaaAnime(xml);

                    if (recentAnime.Count != 0)
                    {
                        var animeChannels = ParseAnimeChannels(recentAnime);

                        if (animeChannels.Count != 0)
                            animeChannels.ForEach(x => x.parsedAnimeChannel.AnimeChannel.CreateOrUpdateMessage(Client,
                                x.NyaaTorrent, x.parsedAnimeChannel.ParsedAnime));
                    }

                    TorrentReminderOptions.SaveReminders(OPTIONS_FILENAME, ReminderOptions);

                    await Task.Delay(TimeSpan.FromMinutes(checkingSpeed));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"oh no :{e.Message}");
                }
        }

        /// <summary>
        ///     Checks if there are any relevant anime in the list that people are subscribed to.
        /// </summary>
        /// <param name="list"></param>
        /// <returns>New episodes of people that are subscribed.</returns>
        private List<TorrentAndParsedChannel> ParseAnimeChannels(List<NyaaTorrent> list)
        {
            var animeList = new List<TorrentAndParsedChannel>();

            foreach (var nAnime in list)
            {
                var parsedTitle = NyaaParser.ParseTitle(nAnime.Title);
                var anime = ReminderOptions.SubscribedAnime.FirstOrDefault(x =>
                    parsedTitle.Title.ToLower().Contains(x.AnimePreference.Name.ToLower()));


                // conditions for adding the anime.
                if (
                    // if the anime is null,,, obviously...
                    anime == null ||
                    // if the this episode is not new... skip it as well.
                    anime.LatestEpisode > parsedTitle.Episode ||
                    // if the parsed subgroup does not contain the wanted subgroup, skip this.
                    anime.AnimePreference.Subgroups.All(x => x.ToLower() != parsedTitle.Fangroup.ToLower())) continue;
                // conditions end.


                animeList.Add(new TorrentAndParsedChannel(nAnime, new ParsedAnimeChannel(parsedTitle, anime)));
            }

            return animeList;
        }

        /// <summary>
        ///     Put the XML item in a NyaaAnime object, with cool fucking information.
        /// </summary>
        /// <param name="doc">what</param>
        /// <returns></returns>
        private List<NyaaTorrent> GetRecentNyaaAnime(XmlDocument doc)
        {
            var list = new List<NyaaTorrent>();
            var channel = doc["rss"]["channel"];
            var childNodes = channel.ChildNodes;
            var @checked = false;
            for (var i = 0; i < childNodes.Count; i++)
            {
                var node = childNodes.Item(i);
                if (node.Name != "item") continue;

                var infoHash = node["nyaa:infoHash"].InnerText;

                // if this torrent entry has already been checked, exit. goodbye.. cunt.
                if (infoHash == ReminderOptions.LatestChecked)
                    return list;


                if (!@checked)
                {
                    ReminderOptions.LatestChecked = infoHash;
                    @checked = true;
                }

                var name = node["title"].InnerText;
                var link = node["guid"].InnerText;
                var s_seeders = node["nyaa:seeders"].InnerText;
                var s_leechers = node["nyaa:leechers"].InnerText;
                var size = node["nyaa:size"].InnerText;
                int seeders, leechers;
                seeders = int.TryParse(s_seeders, out seeders) ? seeders : -1;
                leechers = int.TryParse(s_seeders, out leechers) ? leechers : -1;

                list.Add(new NyaaTorrent
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
        ///     Literally does what the name of it says.
        ///     Gets the RSS feed of Nyaa.si (btw is this like the weeaboo nazi?)
        /// </summary>
        /// <returns></returns>
        private async Task<XmlDocument> GetNyaaRSSAsXML()
        {
            using (var hClient = new HttpClient())
            {
                var message = new HttpRequestMessage(HttpMethod.Get, "https://nyaa.si/?page=rss");
                var asd = await hClient.SendAsync(message);
                var xml = await asd.Content.ReadAsStringAsync();

                var xmlDoc = new XmlDocument();
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