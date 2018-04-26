using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace qtReminder.Nyaa
{
    public partial class TorrentReminder
    {
        public async Task ReceiveNyaaMessage(SocketMessage socketMessage)
        {
            var message = socketMessage as Discord.IMessage;

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
                    await UnsubscribeToAnime(subCommand.Item2.ToLower(), message.Author, message.Channel as ITextChannel);
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
            // do stuff
            return;
        }
        
    }
}