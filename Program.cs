using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using qtReminder.Nyaa;
using qtReminder.Services;

namespace qtReminder
{
    internal class Program
    {
        public static readonly Random Randomizer = new Random();
        public static IServiceProvider ServiceProvider { get; private set; }

        private static void Main(string[] args)
        {
            new Program().RunBot().GetAwaiter().GetResult();
        }

        public async Task RunBot()
        {
            ServiceProvider = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<WaitForQuoteMessageService>()
                .AddSingleton<QuoteService>()
                .BuildServiceProvider();

            var settings = GetSettings();

            Console.WriteLine("Logging in...");

            var client = ServiceProvider.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            client.UserLeft += async user =>
            {
                    await user?.Guild?.DefaultChannel?.SendMessageAsync($"{user?.Username} left {user?.Guild?.Name} 🙁");
            };
            
            ServiceProvider.GetRequiredService<CommandService>().Log += LogAsync;
            
            await client.LoginAsync(TokenType.Bot, (string) settings.token, true);
            await client.StartAsync();
            
            while (client.ConnectionState != ConnectionState.Connected) await Task.Delay(1);

            Console.WriteLine("Logged in...");

            // start the torrent reminder
            
            var reminder = new TorrentReminder(client);
            await Task.Factory.StartNew(() => reminder.RepeatCheck());

            client.ReactionAdded += async (cacheable, channel, reaction) =>
            {
                await Task.Factory.StartNew(() => reminder.ReceiveReaction(cacheable, channel, reaction));
            };

            await ServiceProvider.GetRequiredService<CommandHandlingService>().InitializeAsync();

            await Task.Delay(-1);
        }
        
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private dynamic GetSettings()
        {
            try
            {
                var jsonText = File.ReadAllText("settings.json");
                dynamic json = JObject.Parse(jsonText);
                return json;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("File not found. Exiting program.");
                Environment.Exit(1);
                return null;
            }
        }
    }
}