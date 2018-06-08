using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using qtReminder.ImageSearch;

namespace qtReminder.Models
{
    public class AnimeChannel
    {
        [JsonIgnore] private DateTime _lastMessagePostTime;

        [JsonProperty("anime")]
        public AnimePreference AnimePreference;
        public ulong Channel;
        public ulong Guild;
        public List<Quality> KnownQualities = new List<Quality>();

        [JsonIgnore] public IUserMessage LastMessage;
        [JsonIgnore] public bool alreadyNotified;

        public int LatestEpisode;

        [JsonIgnore] public List<KeyValuePair<Quality, string>> QualityLinks;

        public List<ulong> SubscribedUsers = new List<ulong>();

        public void AddQualityLink(Quality quality, string link)
        {
            if (QualityLinks == null) ResetQualityLinks();

            if (QualityLinks.Any(x => x.Key == quality))
            {
                var index = QualityLinks.FindIndex(x => x.Key == quality);
                QualityLinks[index] = new KeyValuePair<Quality, string>(quality, link);
            }

            QualityLinks.Add(new KeyValuePair<Quality, string>(quality, link));

            if (KnownQualities.Any(x => x == quality)) return;

            KnownQualities.Add(quality);
        }

        public void ResetQualityLinks()
        {
            QualityLinks = new List<KeyValuePair<Quality, string>>();
            alreadyNotified = false;
        }

        /// <summary>
        ///     Sets the message and sets the date time to the curren time.
        /// </summary>
        public void SetMessage(IUserMessage message)
        {
            LastMessage = message;
            _lastMessagePostTime = DateTime.Now;
        }

        /// <summary>
        ///     Checks if the message is still valid. (true if newer than 20 minutes)
        /// </summary>
        public bool MessageValid()
        {
            var timespan = DateTime.Now - _lastMessagePostTime;
            return timespan.TotalMinutes < 20 && LastMessage != null;
        }
    }

    public static class AnimeChannelExtensions
    {
        /// <summary>
        ///     This will post a link to the torrent in the subbed channels, and tag all the user that
        ///     are subscribed to it.
        /// </summary>
        /// <param name="client">The discord client.</param>
        public static void CreateOrUpdateMessage(this AnimeChannel ac, DiscordSocketClient client,
            NyaaTorrent nyaaTorrentObject, ParsedAnime parsedAnime)
        {
            if (ac.LatestEpisode != parsedAnime.Episode && ac.alreadyNotified) ac.ResetQualityLinks();

            ac.AddQualityLink(parsedAnime.Quality, nyaaTorrentObject.Link);

            // If the quality of the torrent is lower than the actual
            // minimum notify quality, dont beep them.
            int minimumQuality = (int)ac.AnimePreference.MinQuality;
            if (minimumQuality > (int) parsedAnime.Quality) return;
            
            var tags = "";
            foreach (var user in ac.SubscribedUsers) tags += $"<@{user}> ";

            // If the last posted message is valid, and if the last message is not null.
            // Then edit the previous message posted.
            // If it is not valid, check if the newly posted episode is equal to or lower than the
            // latest checked episode.
            if (ac.MessageValid())
            {
                ac.LastMessage?.ModifyAsync(x => { x.Embed = ac.CreateEmbed(nyaaTorrentObject, parsedAnime); });
                return;
            }

            if (ac.LatestEpisode >= parsedAnime.Episode) return;

            var guild = client.Guilds.FirstOrDefault(x => x.Id == ac.Guild);
            var channel = guild?.GetChannel(ac.Channel) as ITextChannel;

            if (channel == null) return;
            var message = channel.SendMessageAsync(tags, embed: ac.CreateEmbed(nyaaTorrentObject, parsedAnime))
                .GetAwaiter().GetResult();

            ac.alreadyNotified = true;

            ac.LatestEpisode = parsedAnime.Episode;
            ac.SetMessage(message);
        }

        /// <summary>
        ///     Creates the embed... cool shit.
        /// </summary>
        /// <param name="episode"></param>
        /// <param name="nyaaTorrentObject"></param>
        /// <returns> what? </returns>
        private static Embed CreateEmbed(this AnimeChannel ac, NyaaTorrent nyaaTorrentObject, ParsedAnime parsedAnime)
        {
            var imageUrls = DuckDuckGoImageSearch.SearchImage($"{parsedAnime.Title} anime");
            var imageUrl = imageUrls[Program.Randomizer.Next(imageUrls.Length)];
            
            var description = "";

            foreach (var quality in typeof(Quality).GetEnumValues().Cast<Quality>().ToArray())
            {
                if (quality == Quality.Unknown) continue;

                // see if the quality already has a link.
                var link = ac.QualityLinks.FirstOrDefault(x => x.Key == quality);
                var qualityKnown = ac.KnownQualities.Contains(quality);
                
                // todo: also add the subgroup that uploaded this.
                // honestly I am programming myself into a fucking bean here.
                
                if (link.Value != null)
                    description += $"[{quality.ToString()}]({link.Value})\n";
                else if (qualityKnown) description += $"{quality.ToString()}\n";
            }

            var builder = new EmbedBuilder()
                .WithTitle($"{parsedAnime.Title.FirstLettersToUpper()} - Ep. {parsedAnime.Episode} has been released!")
                .WithDescription(description)
                .WithColor(Color.Red)
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Possible Subgroups";
                    x.Value = String.Join(", ", ac.AnimePreference.Subgroups);
                })
                .WithImageUrl(imageUrl);

            return builder.Build();
        }

        /// <summary>
        ///     Subscribe the user to this anime, in this channel.
        /// </summary>
        /// <param name="user">The user to subscribe.</param>
        /// <returns>true if the user has subscribed, false if the user is already subscribed.</returns>
        public static bool SubscribeUser(this AnimeChannel ac, ulong user)
        {
            if (ac.UserSubscribed(user)) return false;

            ac.SubscribedUsers.Add(user);
            return true;
        }

        /// <summary>
        ///     Check to see if the user is subbed to this anime.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <returns>True or false if the user is subscribed or not.</returns>
        public static bool UserSubscribed(this AnimeChannel ac, ulong user)
        {
            return ac.SubscribedUsers.Contains(user);
        }

        /// <summary>
        ///     Unsubscribe the user from this anime, in this channel.
        /// </summary>
        /// <param name="user">The user to unsub.</param>
        /// <returns>
        ///     True if the unsubbing succeeded, false if the user wasn't subscribed in the first place, or if another error
        ///     has occurred.
        /// </returns>
        public static bool UnsubscribeUser(this AnimeChannel ac, ulong user)
        {
            if (!ac.UserSubscribed(user)) return false;

            return ac.SubscribedUsers.Remove(user);
        }
    }
}