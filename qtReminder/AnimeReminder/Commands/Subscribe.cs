using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using qtReminder.AnimeReminder;
using qtReminder.AnimeReminder.AniList;
using Discord.Commands;
using Discord.WebSocket;
using qtReminder.AnimeReminder.Database;
using qtReminder.AnimeReminder.Models;
using qtReminder.ResponseModule;

namespace qtReminder.AnimeReminder.Commands
{
    /// <summary>
    /// EWWW this class is long :(!
    /// </summary>
    public class Subscribe : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// This function will look for the anime,
        /// and then subscribe to the WaiterModule that waits for their message.
        /// </summary>
        [Command("sub"), Alias("s", "subscribe"),
        Remarks("Subscribe to an anime. Usage: $sub {anime name}")]
        public async Task SubscribeToAnime([Remainder] string args)
        {
            
            if (Context.Guild == null) return;
            
            if (string.IsNullOrEmpty(args))
            {
                await ReplyAsync($"{Context.User.Mention}, uh... you didn't specify an anime. Please do so.");
                return;
            }

            // future proofing for the possibility for extra arguments.
            string anime = args;
            
            // search for the anime.
            AniListModel[] animeList = await AnilistRequest.FindAnime(anime);
            if (animeList.Length == 0) return;
            
            
            GuildUserWaiter guildUserWaiter = new GuildUserWaiter(Context.Guild.Id, Context.User.Id, WaiterFunction, 
                animeList);
            guildUserWaiter.ParentMessage = Context.Message;

            var s = new StringBuilder("What anime?\n```Ini\n");
            for(int i = 0; i < animeList.Length; i++)
            {
                s.AppendLine($"{i + 1} \t= {animeList[i].Title.EnglishTitle ?? animeList[i].Title.RomajiTitle}");
            }

            s.AppendLine("```");

            IMessage msg = await ReplyAsync(s.ToString());
            guildUserWaiter.AddAssociatedMessage(msg);
            ResponseModule.ResponseModule.AddWaiter(guildUserWaiter);
        }

        private async Task<bool> WaiterFunction(SocketMessage message, object @params)
        {
            string content = message.Content;
            int n;
            
            if (!int.TryParse(content, out n)) return false;
            if (!(@params is AniListModel[] animeList)) return false;
            if (n < 1 || n > animeList.Length + 1) return false;
            
            n--;            
            var anim = new AnimeGuildModel()
            {
                AnimeID = animeList[n].ID,
                Anime = animeList[n],
                AnimeTitle = animeList[n].Title,
                Guild = ((IGuildChannel) message.Channel).GuildId,
                Channel = message.Channel.Id,
                MinAnnounceQuality = Quality.SevenTwentyP,
                LastAnnouncedEpisode = 0,
                WantedSubgroupTitle = new [] {"horriblesubs", "erai-raws"}
            };

            if (DatabaseSubscriber.SubscribeToAnime(ref anim, message.Author.Id))
            {
                Embed embed = CreateEmbed(anim, message);
                var succeedMessage = await ReplyAsync("", embed: embed);
                await succeedMessage.AddReactionAsync(new Emoji("❤"));
                
                var guildUserWaiter = new GuildUserWaiter(Context.Guild.Id, Context.User.Id,
                    async (messageId, reaction, anime) =>
                    {
                        if (reaction.Emote.Name != "❤") return false;
                        var a = anim;
                        DatabaseSubscriber.SubscribeToAnime(ref a, reaction.UserId);
                        try
                        {
                            var e = CreateEmbed(a, message);
                            await succeedMessage.ModifyAsync(x => x.Embed = e);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        return false; 
                    }, anim, false);

                guildUserWaiter.ParentMessage = succeedMessage;
                ResponseModule.ResponseModule.AddWaiter(guildUserWaiter);
            }
            else
            {
                await ReplyAsync($"{message.Author.Mention} ばか！　(´-ω-`). You already subscribed to {anim.AnimeTitle.EnglishTitle ?? anim.AnimeTitle.RomajiTitle}.");
            }
            
            return true;
        }

        private Embed CreateEmbed(AnimeGuildModel agm, SocketMessage message)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Wow! You just subscribed to an anime!")
                .WithAuthor(message.Author)
                .WithDescription(
                    $"When a new episode of {agm.AnimeTitle.EnglishTitle ?? agm.AnimeTitle.RomajiTitle} comes out, " +
                    $"you will be notified 😉😉")
                .WithFooter("Click the ❤️ to subscribe to this anime as well 😁!");


            var users = string.Join(" ",GetSubscribedUsers(agm.SubscribedUsers, message.Author.Id));
            if (string.IsNullOrEmpty(users)) users = "Hmmm... It's just you!";
            embedBuilder.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Also subscribed";
                x.Value = $"{users}";
            });

            var a = agm.Anime;
            if (a.NextAiringEpisode?.AiringAt != null)
            {
                // ReSharper disable once PossibleInvalidOperationException
                int b = a.NextAiringEpisode.Value.AiringAt.Value;

                var dateTime = new DateTime(1970, 1, 1)
                    .AddSeconds(b);
                
                
                embedBuilder.AddField(x =>
                {
                    var e = a.NextAiringEpisode.Value.Episode;
                    
                    x.IsInline = true;
                    x.Name = $"Next Episode{(e.HasValue ? $": {e.Value.ToString()}" : "")}";
                    x.Value = $"{dateTime.DayOfWeek.ToString()} ({dateTime.Day} " +
                              $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month)})";
                });
            }

            return embedBuilder.Build();
        }

        private string[] GetSubscribedUsers(ulong[] users, ulong ignore)
        {
            if ((users.Length == 0) || Context.Guild == null) return new [] {""};
            return (
                from user in users 
                where user != ignore 
                select Context.Guild.GetUser(user) into guilduser 
                select guilduser.Nickname ?? guilduser.Username).ToArray();
        }
    }
}