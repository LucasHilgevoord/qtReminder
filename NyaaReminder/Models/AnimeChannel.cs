using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using qtReminder.ImageSearch;
using qtReminder.Nyaa;
using qtReminder.Services;

namespace qtReminder.Models
{
    /// <summary>
    /// An anime channel points to a channel for a specific anime. This includes the preference of quality and
    /// subgroup
    /// </summary>
    public class AnimeChannel
    {
        [JsonIgnore] private DateTime _lastMessagePostTime;

        [JsonProperty("anime")]
        public AnimePreference AnimePreference;
        public ulong Channel;
        public ulong Guild;
        public List<Quality> KnownQualities = new List<Quality>();
        public int LatestEpisode;

        /// <summary>
        /// The actual name of the anime.
        /// </summary>
        [JsonIgnore] public string actualName;
        
        /// <summary>
        /// The notification message.
        /// </summary>
        [JsonIgnore] public IUserMessage LastMessage;
        
        /// <summary>
        /// List of the quality links. Could've as well been a dict. Why didn't I do that?
        /// </summary>
        [JsonIgnore] public Dictionary<Quality, string[]> QualityLinks;
        
        [JsonIgnore] public string thisImage;
        [JsonIgnore] public QuoteService.Quote thisQuote;
        [JsonIgnore] public int thisEpisode;
        
        /// <summary>
        /// This bool is used to check if they've already been notified.
        /// It edits the message if so, and posts a new message if not.
        /// </summary>
        [JsonIgnore] public bool alreadyNotified;

        public List<ulong> SubscribedUsers = new List<ulong>();

        public void AddQualityLink(Quality quality, string link, string subgroup)
        {
            // If the quality links is not yet instantiated, make it.
            if (QualityLinks == null) ResetQualityLinks();

            if (QualityLinks.Any(x => x.Key == quality))
            {
                QualityLinks[quality] = new[] { link, subgroup };
                return;
            }

            QualityLinks.Add(quality ,new[] { link, subgroup });

            if (KnownQualities.Any(x => x == quality)) return;

            KnownQualities.Add(quality);
        }

        public void ResetQualityLinks()
        {
            CreateQualityLinks();
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
        /// Use this function to invalidate this object.
        /// Invalidating this clears the quality links, and generates a new image.
        /// </summary>
        private void Invalidate()
        {
            GenerateNewImage();
            ResetQualityLinks();
            GenerateQuote();
            LatestEpisode++;
            alreadyNotified = false;
        }

        private void GenerateNewImage()
        {
            var imageUrls = DuckDuckGoImageSearch.SearchImage($"{AnimePreference.Name} anime");
            thisImage = imageUrls[Program.Randomizer.Next(imageUrls.Length)];
        }

        private void GenerateQuote()
        {
            // get random quote
            var quote = Program.ServiceProvider.GetRequiredService<QuoteService>().GetRandomQuote();
                    
            string name = string.IsNullOrEmpty(quote.Name) ? "no one" : quote.Name;
            string quoteText = string.IsNullOrWhiteSpace(quote.QuoteText) ? "fuck" : quote.QuoteText;
            
            thisQuote = new QuoteService.Quote(name, quoteText);
        }

        /// <summary>
        ///     Checks if the message is still valid. (true if newer than 20 minutes)
        /// </summary>
        public bool MessageValid()
        {
            var timespan = DateTime.Now - _lastMessagePostTime;
            return timespan.TotalMinutes < 30 && LastMessage != null;
        }

        private void CreateQualityLinks()
        {
            QualityLinks = new Dictionary<Quality, string[]>();
        }

        /// <summary>
        /// Update the anime links if it's the same episode,
        /// if it's a new episode, generate a new image and also clear the link list.
        /// </summary>
        /// <param name="parsedAnime"></param>
        public void UpdateLinks(ParsedAnime parsedAnime)
        {
            if (LatestEpisode < parsedAnime.Episode || QualityLinks == null) Invalidate();
            if (!ShouldAddLink(parsedAnime)) return;
            
            actualName = parsedAnime.Title; // set anime title
            AddQualityLink(parsedAnime.Quality, parsedAnime.Link, parsedAnime.Fangroup);
            thisEpisode = parsedAnime.Episode;
        }

        /// <summary>
        /// Checks if the link should be added.
        /// Right now it only does it based on sub group. Lowest first.
        /// </summary>
        private bool ShouldAddLink(ParsedAnime parsedAnime)
        {
            if (!QualityLinks.TryGetValue(parsedAnime.Quality, out var array)) return true;
            
            int currentSubgroup = AnimePreference.Subgroups.ToList().FindIndex(x => array[1].ToLower() == x.ToLower());
            int newSubgroup = AnimePreference.Subgroups.ToList()
                .FindIndex(x => parsedAnime.Fangroup.ToLower() == x.ToLower());

            return newSubgroup < currentSubgroup;

        }
    }

    public static class AnimeChannelExtensions
    {
        /// <summary>
        ///     This will post a link to the torrent in the subbed channels, and tag all the user that
        ///     are subscribed to it.
        /// </summary>
        /// <param name="client">The discord client.</param>
        public static void CreateOrUpdateMessage(this AnimeChannel ac, DiscordSocketClient client)
        {
            var tags = "";
            foreach (var user in ac.SubscribedUsers) tags += $"<@{user}> ";

            // If the last posted message is valid, and if the last message is not null.
            // Then edit the previous message posted.
            // If it is not valid, check if the newly posted episode is equal to or lower than the
            // latest checked episode.
            if (ac.MessageValid())
            {
                ac.LastMessage?.ModifyAsync(x => { x.Embed = ac.CreateEmbed(); });
                return;
            }

            if (ac.alreadyNotified) return;

            var guild = client.Guilds.FirstOrDefault(x => x.Id == ac.Guild);
            if (!(guild?.GetChannel(ac.Channel) is ITextChannel channel)) return;
            var message = channel.SendMessageAsync(tags, embed: ac.CreateEmbed())
                .GetAwaiter().GetResult();

            Program.ServiceProvider.GetRequiredService<WaitForQuoteMessageService>()
                .WaitForMessageInChannel(channel.Id).Wait();

            ac.alreadyNotified = true;
            ac.SetMessage(message);
        }

        /// <summary>
        ///     Creates the embed... cool shit.
        /// </summary>
        /// <param name="episode"></param>
        /// <param name="nyaaTorrentObject"></param>
        /// <returns> what? </returns>
        private static Embed CreateEmbed(this AnimeChannel ac)
        {            
            var description = "";

            foreach (var quality in typeof(Quality).GetEnumValues().Cast<Quality>().ToArray())
            {
                if (quality == Quality.Unknown) continue;

                // see if the quality already has a link.
                var link = ac.QualityLinks.FirstOrDefault(x => x.Key == quality);
                var qualityKnown = ac.KnownQualities.Contains(quality);
                
                if (link.Value != null)
                    description += $"[{link.Value[1]} - {quality.ToString()}]({link.Value[0]})\n";
                else if (qualityKnown) description += $"{quality.ToString()}\n";
            }

            var builder = new EmbedBuilder()
                .WithTitle($"{ac.actualName.FirstLettersToUpper()} - Ep. {ac.thisEpisode} just came out!")
                .WithDescription(description)
                .WithColor(Color.Red)
                .AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = $"And as {ac.thisQuote.Name} would say";
                    x.Value = ac.thisQuote.QuoteText;
                })
                .WithImageUrl(ac.thisImage);

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
            return ac.UserSubscribed(user) && ac.SubscribedUsers.Remove(user);
        }
    }
}