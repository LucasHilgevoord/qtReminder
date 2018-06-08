using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using qtReminder.Nyaa;

namespace qtReminder
{
    internal class Program
    {
        public static DiscordSocketClient Client { get; private set; }
        public static readonly Random Randomizer = new Random();

        private static void Main(string[] args)
        {
            new Program().RunBot().GetAwaiter().GetResult();
        }

        public static bool IsMe(ulong Id)
        {
            return Id == Client.CurrentUser.Id;
        }

        public async Task RunBot()
        {
            Client = new DiscordSocketClient();

            var settings = GetSettings();

            Console.WriteLine("Logging in...");


            await Client.LoginAsync(TokenType.Bot, (string) settings.token, true);
            await Client.StartAsync();
            while (Client.ConnectionState != ConnectionState.Connected) await Task.Delay(1);

            Console.WriteLine("Logged in...");

            var reminder = new TorrentReminder(Client);
            await Task.Factory.StartNew(() => reminder.RepeatCheck());

            Client.MessageReceived += async msg =>
            {
                await Task.Factory.StartNew(() => reminder.ReceiveNyaaMessage(msg));
            };

            Client.ReactionAdded += async (cacheable, channel, reaction) =>
            {
                await Task.Factory.StartNew(() => reminder.ReceiveReaction(cacheable, channel, reaction));
            };

            Console.WriteLine("Running bot");
            await Task.Delay(-1);
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