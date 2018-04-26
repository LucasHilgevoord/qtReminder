using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using qtReminder.Models;

namespace qtReminder.Nyaa
{
    public partial class TorrentReminder
    {
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
        
        private async Task SubscribeToAnime(string animeTitle, IUser user, ITextChannel channel)
        {
            // Helper function for sending a message if the user has been subscribed.
            async Task SendSubscribeMessage(AnimeChannel anime)
            {
                List<string> subscribedUsers = new List<string>();

                foreach (var uID in anime.SubscribedUsers)
                {
                    var guildUser = await channel.Guild.GetUserAsync(uID);
                    string username = String.IsNullOrEmpty(guildUser?.Nickname) ? guildUser?.Username : guildUser?.Nickname;
                    if (username != null) subscribedUsers.Add(username);
                }
                
                EmbedBuilder embedBuilder = new EmbedBuilder()
                    .WithAuthor(user)
                    .WithDescription($"You have been subscribed to {anime.Anime.Name.FirstLettersToUpper()}.")
                    .AddField("Subgroup", anime.Anime.Subgroup, true)
                    .AddField("Quality", anime.Anime.MinQuality.ToString())
                    .WithColor(Color.Green)
                    .WithFooter("🔴 : sub to this too.");

                if (subscribedUsers.Count != 0)
                {
                    string s = String.Join(", ", subscribedUsers);
                    if (s.Length > EmbedBuilder.MaxFieldCount)
                        s = s.Substring(0, EmbedBuilder.MaxFieldCount - 3) + "..";
                    embedBuilder.AddField("Also subscribed", String.Join(", ", subscribedUsers));
                }
                    
                
                var embed = embedBuilder.Build();

                var message = await channel.SendMessageAsync("", embed: embed);
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
    }
}