using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using qtReminder.Nyaa;

namespace qtReminder.Services
{
    public class WaitForQuoteMessageService
    {
        private readonly DiscordSocketClient _socketClient;
        private readonly QuoteService _quoteService;

        public WaitForQuoteMessageService(IServiceProvider serviceProvider)
        {
            _socketClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
            _quoteService = serviceProvider.GetRequiredService<QuoteService>();
        }

        public async Task WaitForMessageInChannel(ulong channel)
        {
            bool foundMessage = false;
            SocketMessage message = null;
            
            Task SocketClientOnMessageReceived(SocketMessage socketMessage)
            {
                if (socketMessage.Channel.Id != channel || _socketClient.CurrentUser.Id == socketMessage.Author.Id) return Task.CompletedTask;

                message = socketMessage;
                foundMessage = true;

                return Task.CompletedTask;
            }
            
            _socketClient.MessageReceived += SocketClientOnMessageReceived;

            await Task.Factory.StartNew(() =>
            {
                SpinWait.SpinUntil(() => foundMessage);
                _socketClient.MessageReceived -= SocketClientOnMessageReceived;

                if (!(message.Channel is IGuildChannel guild)) return;
                _quoteService.AddQuote(message.Author.Id, guild.GuildId, message.Content);
            });
        }

        
    }
}