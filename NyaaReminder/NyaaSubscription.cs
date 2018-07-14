using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using qtReminder.Models;

namespace qtReminder.Nyaa
{
    public partial class TorrentReminder
    {
        public static List<SubscribeMessage> subscribeMessages = new List<SubscribeMessage>();
        
        /// <summary>
        ///     Will look at the second word, to see if it's subscribe or unsubscribe.
        ///     too lazy to make an enum for this.
        /// </summary>
        /// <param name="content">content of the message</param>
        /// <returns>
        ///     returns -1 if nothing was found, 1 if it's subscribe, or 0 if it's unsubscribe. Together the first two words
        ///     cut out.
        /// </returns>
        private (int, string) SubscribeCommand(string content)
        {
            var splitContent = content.Split(" ");

            if (splitContent.Length < 3) return (-1, null);

            var returnString = content.Substring(splitContent[0].Length + splitContent[1].Length + 2).Trim();

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

        /// <summary>
        /// This makes the anime subscription with the specified requirements. 
        /// </summary>
        /// <param name="anime">ehh??</param>
        /// <param name="user">If a user is specified, it will make add the author to the embed. Very cool.</param>
        /// <returns></returns>
        public static async Task<Embed> MakeAnimeSubscriptionEmbed(AnimeChannel anime, IUser user = null)
        {
            var subscribedUsers = new List<string>();
            var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();
            var Guild = client.GetGuild(anime.Guild) as IGuild;

            foreach (var uID in anime.SubscribedUsers)
            {
                var guildUser = await Guild.GetUserAsync(uID);
                var username = string.IsNullOrEmpty(guildUser?.Nickname) ? guildUser?.Username : guildUser?.Nickname;
                if (username != null) subscribedUsers.Add(username);
            }

            var embedBuilder = new EmbedBuilder()
                .WithDescription($"You have been subscribed to {anime.AnimePreference.Name.FirstLettersToUpper()}.")
                .AddField("Subgroup(s)", string.Join(", ", anime.AnimePreference.Subgroups), true)
                .AddField("Minimum Quality", anime.AnimePreference.MinQuality.ToString())
                .WithColor(Color.Green)
                .WithFooter("🔴 : sub to this too.");

            if (user != null) embedBuilder.WithAuthor(user);

            if (subscribedUsers.Count != 0)
            {
                var s = string.Join(", ", subscribedUsers);
                if (s.Length > EmbedBuilder.MaxFieldCount)
                    s = s.Substring(0, EmbedBuilder.MaxFieldCount - 3) + "..";
                embedBuilder.AddField("Also subscribed", string.Join(", ", subscribedUsers));
            }

            return embedBuilder.Build();
        }

        private async Task SubscribeToAnime(string animeTitle, IUser user, ITextChannel channel)
        {
            // Helper function for sending a message if the user has been subscribed.
            async Task SendSubscribeMessage(AnimeChannel anime)
            {
                var embed = await MakeAnimeSubscriptionEmbed(anime, user);
                var message = await channel.SendMessageAsync("", embed: embed);
                var submessage = new SubscribeMessage(anime, message);
                subscribeMessages.Add(submessage);
                submessage.Disposed += () =>
                {
                    try
                    {
                        subscribeMessages.Remove(submessage);
                    }
                    catch (Exception)
                    {
                        // ligma nuts! lole
                    }
                };
                await message.AddReactionAsync(new Emoji("🔴"));
            }

            // check if this server is already subscribed to this anime...
            // if not, make it!! yes!!!
            // if he is, subscribe him to it.

            foreach (var anime in ReminderOptions.SubscribedAnime)
            {
                if (!anime.AnimePreference.Name.ToLower().Contains(animeTitle) ||
                    anime.Guild != channel.GuildId) continue;

                var succeeded = anime.SubscribeUser(user.Id);

                if (!succeeded)
                {
                    await channel.SendMessageAsync($"{user.Mention}, you dumb fucking piece of shit, you are already subscribed to this.");
                    return;
                }

                await SendSubscribeMessage(anime);
                return;
            }

            var animePreferenceSub = new AnimePreference(animeTitle);
            var animeToSubscribe = new AnimeChannel
            {
                Guild = channel.GuildId,
                Channel = channel.Id,
                AnimePreference = animePreferenceSub
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

            if (ReminderOptions.SubscribedAnime.Count == 0)
            {
                await channel.SendMessageAsync(
                    "There are no subscriptions anywhere. You fucking braindead idiot. Fuck you.");
                return;
            }

            foreach (var anime in ReminderOptions.SubscribedAnime)
            {
                if (anime.Guild != channel.GuildId || anime.Channel != channel.Id ||
                    !anime.AnimePreference.Name.ToLower().Contains(animeTitle)) return;

                anime.UnsubscribeUser(user.Id);

                if (anime.SubscribedUsers.Count == 0) ReminderOptions.SubscribedAnime.Remove(anime);

                var message = await channel.SendMessageAsync(
                    $"{user.Mention} You have been unsubscribed from " +
                    $"{anime.AnimePreference.Name}\n\n\nFucking asshole.");

                await Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(1500);
                    await message.ModifyAsync(x => x.Content = $"{user.Mention} You have been unsubscribed from " +
                                                         $"{anime.AnimePreference.Name}");
                });
            }
        }
    }
}