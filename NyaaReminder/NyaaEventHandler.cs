using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace qtReminder.Nyaa
{
    public partial class TorrentReminder : ModuleBase<SocketCommandContext>
    {
        [Command("subscribe"), Alias("sub")]
        public async Task SubscribeUser([Remainder] string text)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("No");
                return;
            }
            
            await SubscribeToAnime(text.ToLower(), Context.User, Context.Channel as ITextChannel);
            
            TorrentReminderOptions.SaveReminders(OPTIONS_FILENAME, ReminderOptions, true);
        }
        
        [Command("unsubscribe"), Alias("unsub")]
        public async Task UnsubscribeUser([Remainder] string text)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync("No");
                return;
            }
            
            await UnsubscribeToAnime(text.ToLower(), Context.User, Context.Channel as ITextChannel);

            TorrentReminderOptions.SaveReminders(OPTIONS_FILENAME, ReminderOptions, true);
        }

        public async Task ReceiveReaction(Cacheable<IUserMessage, ulong> a, ISocketMessageChannel b,
            SocketReaction reaction)
        {
            var subscribeMessage = TorrentReminder.subscribeMessages.FirstOrDefault(x => x.userMessage.Id == a.Id);

            bool IsMe = base.Context.Client.CurrentUser.Id == reaction.UserId;
            
            // if the subscribe message is not null and the emote is the right one... then I suppose we can?
            // just subscribe him? is it really that easy?
            if (subscribeMessage == null || reaction.Emote.Name != "🔴" ||
                IsMe) return;

            await subscribeMessage.UserSubscribe(reaction.UserId);
        }
    }
}