using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Discord;
using Discord.WebSocket;

namespace qtReminder.Models
{
    public class AnimeChannel
    {
        public ulong Guild;
        public ulong Channel;
        public List<ulong> SubscribedUsers = new List<ulong>();
        public Anime Anime;
        public NyaaAnime CurrentAnimeTorrent;
        public int LatestEpisode;
    }
    
    public static class AnimeChannelExtensions
    {
        /// <summary>
        /// This will post a link to the torrent in the subbed channels, and tag all the user that
        /// are subscribed to it.
        /// </summary>
        /// <param name="client">The discord client.</param>
        public static void NotifyUsers(this AnimeChannel ac, DiscordSocketClient client, 
            int episode, NyaaAnime nyaaAnimeObject, ParsedAnime parsedAnime)
        {
            var guild = client.Guilds.FirstOrDefault(x => x.Id == ac.Guild); // 172734552119312384
            var channel = guild.GetChannel(ac.Channel) as ITextChannel;

            const bool useCoolEmbed = true;

            if (useCoolEmbed)
                channel?.SendMessageAsync("", false, ac.CreateEmbed(episode, nyaaAnimeObject, parsedAnime));
            else
                channel?.SendMessageAsync($"{parsedAnime.Title} episode {episode} has been released!");
            
            string tags = "";
            foreach (var user in ac.SubscribedUsers)
            {
                tags += $"<@{user}> ";
            }

            channel?.SendMessageAsync(tags);

            ac.LatestEpisode = episode;
        }

        /// <summary>
        /// Creates the embed... cool shit.
        /// </summary>
        /// <param name="episode"></param>
        /// <param name="nyaaAnimeObject"></param>
        /// <returns> what? </returns>
        private static Embed CreateEmbed(this AnimeChannel ac, int episode, NyaaAnime nyaaAnimeObject, ParsedAnime parsedAnime)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle($"{parsedAnime.Title.FirstLettersToUpper()} - Ep. {parsedAnime.Episode} has been released!")
                .AddField("size", nyaaAnimeObject.Size, true)
                .WithUrl(nyaaAnimeObject.Link)
                .WithColor(Color.Red)
                .AddField("Sub group", ac.Anime.Subgroup, true);

            return builder.Build();
        }
        
        /// <summary>
        /// Subscribe the user to this anime, in this channel.
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
        /// Check to see if the user is subbed to this anime.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <returns>True or false if the user is subscribed or not.</returns>
        public static bool UserSubscribed(this AnimeChannel ac, ulong user)
        {
            return ac.SubscribedUsers.Contains(user);
        }

        /// <summary>
        /// Unsubscribe the user from this anime, in this channel.
        /// </summary>
        /// <param name="user">The user to unsub.</param>
        /// <returns>True if the unsubbing succeeded, false if the user wasn't subscribed in the first place, or if another error has occurred.</returns>
        public static bool UnsubscribeUser(this AnimeChannel ac, ulong user)
        {
            if (!ac.UserSubscribed(user)) return false;

            return ac.SubscribedUsers.Remove(user);
        }
    }
}