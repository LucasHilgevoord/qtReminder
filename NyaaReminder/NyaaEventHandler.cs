using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace qtReminder.Nyaa
{
    public partial class TorrentReminder
    {
        public async Task ReceiveNyaaMessage(SocketMessage socketMessage)
        {
            var message = socketMessage as IMessage;

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
                    await UnsubscribeToAnime(subCommand.Item2.ToLower(), message.Author,
                        message.Channel as ITextChannel);
                    break;
                default:
                    await message.Channel.SendMessageAsync(" what ");
                    break;
            }

            TorrentReminderOptions.SaveReminders(OPTIONS_FILENAME, ReminderOptions, true);
        }

        public async Task ReceiveReaction(Cacheable<IUserMessage, ulong> a, ISocketMessageChannel b,
            SocketReaction reaction)
        {
            var subscribeMessage = TorrentReminder.subscribeMessages.FirstOrDefault(x => x.userMessage.Id == a.Id);

            // if the subscribe message is not null and the emote is the right one... then I suppose we can?
            // just subscribe him? is it really that easy?
            if (subscribeMessage == null || reaction.Emote.Name != "🔴" ||
                Program.IsMe(reaction.UserId)) return;

            await subscribeMessage.UserSubscribe(reaction.UserId);
        }
    }
}