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
    public class TorrentReminder
    {
        private const string OPTIONS_FILENAME = "nyaaoptions.json";
        
        private DiscordSocketClient Client { get; }
        private ReminderOptions ReminderOptions { get; set; }

        public TorrentReminder(DiscordSocketClient client)
        {
            Client = client;
            ReminderOptions = TorrentReminderOptions.LoadReminders(OPTIONS_FILENAME);
        }

        public async Task ReceiveNyaaMessage(SocketMessage socketMessage)
        {
            var message = socketMessage as Discord.IMessage;

            if (!message.MentionedUserIds.Contains(Client.CurrentUser.Id)) return;
            
            var subCommand = SubscribeCommand(message.Content);

            if (subCommand.Item1 == -1) return;
            
            // Check if the channel the message was sent in is public.
            if (!(message.Channel is IGuildChannel))
            {
                await message.Channel.SendMessageAsync("no");
                return;
            }

            switch (subCommand.Item1)
            {
                case 1:
                    await SubscribeToAnime(subCommand.Item2.ToLower(), message.Author, message.Channel as ITextChannel);
                    break;
                case 0:
                    await UnsubscribeToAnime(subCommand.Item2.ToLower(), message.Author, message.Channel as ITextChannel);
                    break;
                default:
                    await message.Channel.SendMessageAsync(" what ");
                    break;
            }
                
            TorrentReminderOptions.SaveReminders(OPTIONS_FILENAME, ReminderOptions, true);
        }

        private async Task SubscribeToAnime(string animeTitle, IUser user, ITextChannel channel)
        {
            // Helper function for sending a message if the user has been subscribed.
            async Task SendSubscribeMessage(AnimeChannel anime)
            {
                var message = await channel.SendMessageAsync(
                    $"{user.Mention}, you have been subscribed to {anime.Anime.Name}. " +
                    $"And will get notifications if I detect any new torrents by that name!\n" +
                    $"`subgroup: {anime.Anime.Subgroup} & quality: {anime.Anime.MinQuality.ToString()}`\n" +
                    $"Click the `🔴` to subscribe to this as well. `error: not yet implemented you fucking braindead idiot.`");
                await message.AddReactionAsync(new Emoji("🔴"));
            }

            // check if this server is already subscribed to this anime...
            // if not, make it!! yes!!!
            // if he is, subscribe him to it.

            foreach (var anime in ReminderOptions.SubscribedAnime)
            {
                if (!anime.Anime.Name.ToLower().Contains(animeTitle) || anime.Guild != channel.GuildId) continue;
                
                bool succeeded = anime.SubscribeUser(user.Id);

                if (!succeeded)
                {
                    await channel.SendMessageAsync($"{user.Mention}     `Something we`nt fucking wroṇ̦͔̈̍̑g!!");
                    return;
                }

                await SendSubscribeMessage(anime);
                return;
            }
            
            Anime animeSub = new Anime(animeTitle);
            AnimeChannel animeToSubscribe = new AnimeChannel()
            {
                Guild = channel.GuildId,
                Channel = channel.Id,
                Anime = animeSub
            };
            //channel.GuildId, channel.Id, animeSub)
            animeToSubscribe.SubscribeUser(user.Id);
            ReminderOptions.SubscribedAnime.Add(animeToSubscribe);
            
            await SendSubscribeMessage(animeToSubscribe);
            
        }

        private async Task UnsubscribeToAnime(string animeTitle, IUser user, ITextChannel channel)
        {
            // check if this server is subscribed to this anime
            // if not, what the fuck are you doing here? Fucking balbino. Stupid fucking idiot.

            if (ReminderOptions.SubscribedAnime.Count != 0)
            {
                await channel.SendMessageAsync("There are no subscriptions anywhere. You fucking braindead idiot. Fuck you.");
                return;
            }

            foreach (var anime in ReminderOptions.SubscribedAnime)
            {
                if (anime.Guild != channel.GuildId || !anime.Anime.Name.ToLower().Contains(animeTitle)) return;

                anime.UnsubscribeUser(user.Id);

                if (anime.SubscribedUsers.Count == 0)
                {
                    ReminderOptions.SubscribedAnime.Remove(anime);
                }

                await channel.SendMessageAsync(
                    $"{user.Mention} You have been unsubscribed from receiving new notifications when a new episode of " +
                    $"{anime.Anime.Name} goes online!");
            }
        }

        /// <summary>
        /// Will look at the second word, to see if it's subscribe or unsubscribe.
        /// too lazy to make an enum for this.
        /// </summary>
        /// <param name="content">content of the message</param>
        /// <returns>returns -1 if nothing was found, 1 if it's subscribe, or 0 if it's unsubscribe. Together the first two words cut out.</returns>
        private (int, string) SubscribeCommand(string content)
        {
            string[] splitContent = content.Split(" ");

            if (splitContent.Length < 3) return (-1, null);

            string returnString = content.Substring(splitContent[0].Length + splitContent[1].Length + 2).Trim();

            switch (splitContent[1].ToLower())
            {
                case "sub":
                case "subscribe":
                    return (1, returnString);
                case "unsub":
                case "unsubscribe":
                    return (0, returnString);
                default:
                    return (-1, returnString);
            }
        }
        
        public async Task RepeatCheck()
        {
            while (true)
            {
                const int CheckEvery_Minute = 2;
                
                if (ReminderOptions.SubscribedAnime.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    continue;
                }
                
                Console.WriteLine("Checking anime ...");
                var xml = await GetNyaaRSSAsXML();
                var recentAnime = GetRecentNyaaAnime(xml);

                if (recentAnime.Count != 0)
                {
                    var animeChannels = ParseAnimeChannels(recentAnime);

                    if (animeChannels.Count != 0)
                    {
                        Console.WriteLine($"New anime spotted for {animeChannels.Count} channels! Notifying them all!");
                        animeChannels.ForEach(x => x.AnimeChannel.NotifyUsers(Client, x.ParsedAnime.Episode, 
                            x.AnimeChannel.CurrentAnimeTorrent, x.ParsedAnime));
                    }
                }

                TorrentReminderOptions.SaveReminders(OPTIONS_FILENAME, ReminderOptions, true);
                
                await Task.Delay(TimeSpan.FromMinutes(CheckEvery_Minute));
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