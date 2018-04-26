using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using qtReminder.Models;

namespace qtReminder
{
    class Program
    {
        static void Main(string[] args) => new Program().RunBot().GetAwaiter().GetResult();


        private DiscordSocketClient _client;

        public async Task RunBot()
        {
            _client = new DiscordSocketClient();
 
            var settings = GetSettings();
            
            Console.WriteLine("Logging in...");

            
            await _client.LoginAsync(TokenType.Bot, (string) settings.token, true);
            await _client.StartAsync();
            while (_client.ConnectionState != ConnectionState.Connected)
            {
                await Task.Delay(1);
            }

            Console.WriteLine("Logged in...");
            
            var reminder = new Nyaa.TorrentReminder(_client);
            await Task.Factory.StartNew(() => reminder.RepeatCheck());

            _client.MessageReceived += async (msg) =>
            {
                await Task.Factory.StartNew(() => reminder.ReceiveNyaaMessage(msg));
            };

            _client.ReactionAdded += async (cacheable, channel, reaction) =>
            {
                //await Task.Factory.StartNew(() => )
            };
            
            Console.WriteLine("Running bot");
            await Task.Delay(-1);
        }

        public dynamic GetSettings()
        {
            try
            {
                var jsonText = System.IO.File.ReadAllText("settings.json");
                dynamic json = JObject.Parse(jsonText);
                return json;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                Console.WriteLine("File not found. Exiting program.");
                Environment.Exit(1);
                return null;
            }
        }
    }
}