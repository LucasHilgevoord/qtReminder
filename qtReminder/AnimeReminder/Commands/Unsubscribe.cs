using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using qtReminder.AnimeReminder.Database;
using qtReminder.AnimeReminder.Models;
using qtReminder.ResponseModule;

namespace qtReminder.AnimeReminder.Commands
{
    public class Unsubscribe : ModuleBase<SocketCommandContext>
    {
        [Command("unsub"), Alias("unsubscribe", "u")]
        public async Task Unsub([Remainder] string text)
        {
            // This command can only be used within guilds.
            if (Context.Guild == null) return;

            if (string.IsNullOrEmpty(text))
            {
                await ReplyAsync($"{Context.User.Mention}, ばか.. what do you want to unsubscribe from?");
                return;
            }

            string anime = text.ToLower();
            
            // get all anime in this guild and from this user.
            var col = Database.Database.GetDatabaseAndSubscriptionCollection().collection;
            var asd = col.FindAll().ToList();
            var guildAnimeCollection = asd
                .Where(x => 
                    x.Guild == Context.Guild.Id && 
                    x.SubscribedUsers.Contains(Context.User.Id)                  
                    ).ToList();

            if (guildAnimeCollection.Count == 0)
            {
                await ReplyAsync($"{Context.User.Mention}, you're not subscribed to any anime.");
                return;
            }
            
            // get the candidates
            var unsubCandidates = guildAnimeCollection.Where(x =>
                x.AnimeTitle.EnglishTitle.ToLower().Contains(anime) ||
                x.AnimeTitle.RomajiTitle.ToLower().Contains(anime)).Take(10).ToList();

            if (unsubCandidates.Count == 0)
            {
                await ReplyAsync($"{Context.User.Mention}, no anime found with that name.");
                return;
            }

            var guildUserWaiter =
                new GuildUserWaiter(Context.Guild.Id, Context.User.Id, 
                    UnsubscribeWaiter, unsubCandidates);
            guildUserWaiter.ParentMessage = Context.Message;

            var s = new StringBuilder("What anime?\n```Ini\n");
            for (int i = 0; i < unsubCandidates.Count; i++)
            {
                s.AppendLine(
                    $"{i + 1} \t= {unsubCandidates[i].AnimeTitle.EnglishTitle ?? unsubCandidates[i].AnimeTitle.RomajiTitle}");
            }

            s.AppendLine("```");

            var msg = await ReplyAsync(s.ToString());
            guildUserWaiter.AddAssociatedMessage(msg);
            ResponseModule.ResponseModule.AddWaiter(guildUserWaiter);
        }

        private async Task<bool> UnsubscribeWaiter(SocketMessage socketMessage, object o)
        {
            string content = socketMessage.Content;

            if (!int.TryParse(content, out int n)) return false;
            if (!(o is List<AnimeGuildModel> subscribedAnime)) return false;
            if (n < 1 || n > subscribedAnime.Count + 1) return false;

            n--;

            var a = subscribedAnime[n];

            if (!DatabaseSubscriber.UnsubscribeFromAnime(ref a, socketMessage.Author.Id))
            {
                await ReplyAsync($"{Context.User.Mention}, oh no! Something went wrong.");
                return false;
            }

            await ReplyAsync(
                $"{Context.User.Mention}. You have unsubscribed from {a.AnimeTitle.EnglishTitle ?? a.AnimeTitle.RomajiTitle}!");
            
            return true;
        }
    }
}