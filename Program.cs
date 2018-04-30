using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
            
            _client.GuildAvailable += async (guild) =>
            {
                if (guild.Id != 172734552119312384) return;
                
                foreach (var user in guild.Users)
                {
                    try
                    {
                        if (user.Roles.Any(x=>x.Id == 361575094440689674)) continue;
                        await user.ModifyAsync(x =>
                        {
                            var roles = x.RoleIds.Value.ToList();
                            roles.Add(361575094440689674);
                            x.RoleIds = roles;
                        });
                        Console.WriteLine($"Added role {user.Guild.GetRole(361575094440689674)?.Name} to {user.Username}");
                    }
                    catch (Exception)
                    {
                        continue; 
                    }
                }
            };
            

            _client.GuildMemberUpdated += async (olduser, user) =>
            {
                if (user.Guild.Id != 172734552119312384) return;

                await Task.Factory.StartNew(() => UpdateUser(user));
            };

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

        private async Task UpdateUser(SocketGuildUser user)
        {
            try
            {
                if (user.Roles.Any(x => x.Id == 361575094440689674)) return;
                var role = user.Guild.GetRole(361575094440689674);
                await user.AddRoleAsync(role);
                Console.WriteLine($"Added role {role.Name} to {user.Username}");
            }
            catch (Exception ex)
            {
                return;
            }
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