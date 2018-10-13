using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using qtReminder.AnimeReminder.AniList;
using qtReminder.AnimeReminder.Services;
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
                .BuildServiceProvider();
            
            // create settings
            var settings = new qtReminder.Settings.SettingsHelper("settings.config");

            var client = ServiceProvider.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;
            ServiceProvider.GetRequiredService<CommandService>().Log += LogAsync;
            client.MessageReceived += ResponseModule.ResponseModule.MessageReceived;
            client.ReactionAdded += ResponseModule.ResponseModule.OnReactionAdded;
            
            await client.LoginAsync(TokenType.Bot, settings.SettingsModel.ClientToken);
            await client.StartAsync();
            
            while (client.ConnectionState != ConnectionState.Connected) await Task.Delay(1);
            
            await ServiceProvider.GetRequiredService<CommandHandlingService>().InitializeAsync();
            AnimeReminderHandler.StartCheck();
            
            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }
    }
}