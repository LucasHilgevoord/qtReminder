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
        
        public async Task ReceiveNyaaMessage([Remainder] string text)
        {
            var subCommand = SubscribeCommand(text);

            if (subCommand.Item1 == -1) return;

            // Check if the channel the message was sent in is public.
            if (Context.Guild == null)
            {
                await ReplyAsync("no");
                return;
            }

            var user = Context.User;
            var textChannel = Context.Channel as ITextChannel;
            
            switch (subCommand.Item1)
            {
                case 1:
                    await SubscribeToAnime(subCommand.Item2.ToLower(), user, textChannel);
                    break;
                case 0:
                    await UnsubscribeToAnime(subCommand.Item2.ToLower(), user, textChannel);
                    break;
                default:
                    await ReplyAsync(" what you dumbass? ");
                    break;
            }

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