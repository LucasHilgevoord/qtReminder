using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace qtReminder.NounSpammer
{
    public class NounSpammerModule : ModuleBase
    {
        private static bool enabled = false;
        private static bool cyclingWords;
        private static string randomNoun = "";
        private static ulong guildid = 172734552119312384;
        private SocketTextChannel channel;
        private static IRole everyboner;
        
        public async Task Spam()
        {
            await channel.SendMessageAsync($"YOU DUMBASS!!! YOU TRIGGERED THE SECRET WORD, {randomNoun.ToUpper()}!!! ");
            await Task.Delay(200);
            await channel.SendMessageAsync($"YOU'RE FUCKING STUPID!!!!!! DUMB NIGGA.");
            await Task.Delay(200);
            
            for (int i = 0; i < 50; i++)
            {
                string[] assd = {"MEAT", "KILL", "RAPE", "HIT WOMEN!!!!!", "FUUUUUCK!!!", "GOOOOOOD!!!!!!"};
                await channel.SendMessageAsync($"{everyboner.Mention} {assd[Program.Randomizer.Next(assd.Length)]}\n" +
                                               $"{everyboner.Mention} {assd[Program.Randomizer.Next(assd.Length)]}\n" +
                                               $"{everyboner.Mention} {assd[Program.Randomizer.Next(assd.Length)]}\n" +
                                               $"{everyboner.Mention} {assd[Program.Randomizer.Next(assd.Length)]}\n" +
                                               $"{everyboner.Mention} {assd[Program.Randomizer.Next(assd.Length)]}\n" +
                                               $"{everyboner.Mention} {assd[Program.Randomizer.Next(assd.Length)]}");
                await Task.Delay(800);
            }

            enabled = false;
            await EnableSpammerBot();
        }


        [Command("code word")]
        public async Task EnableSpammerBot()
        {
            if (enabled)
            {
                await ReplyAsync("CODE WORD IS ALREADY ENABLED LIBTARD!!! TRY AGAIN");
                return;
            }

            enabled = true;
            randomNoun = GetRandomSong();
            Console.WriteLine(randomNoun);
            
            await Task.Factory.StartNew(async () =>
            {
                bool keywordsaid = false;
                
                async Task WaitForWord(SocketMessage message)
                {
                    if (!(message.Channel is IGuildChannel channel)) return;

                    keywordsaid = channel.GuildId == guildid &&
                                  message.Content.ToLower().Contains(randomNoun.ToLower());

                    if (keywordsaid)
                    {
                        this.channel = Program.ServiceProvider.GetService<DiscordSocketClient>().GetGuild(guildid)
                            .GetChannel(message.Channel.Id) as SocketTextChannel;

                        everyboner = channel.Guild.Roles.First(x => x.Name.ToLower() == "everyboner");
                        
                        Program.ServiceProvider.GetService<DiscordSocketClient>().MessageReceived -= WaitForWord;
                    }
                }

                Program.ServiceProvider.GetService<DiscordSocketClient>().MessageReceived += WaitForWord;
                if (!cyclingWords)
                {
                    await Task.Factory.StartNew(async () =>
                    {
                        async Task Wait()
                        {
                            await Task.Delay(TimeSpan.FromDays(1));
                            randomNoun = GetRandomSong();
                            Console.WriteLine(randomNoun);
                            await Wait();
                        }

                        await Wait();
                    });

                    cyclingWords = true;
                }

                SpinWait.SpinUntil(() => keywordsaid);
                await Spam();
            });
        }

        private string GetRandomSong()
        {
            var text = File.ReadAllText("nouns.txt");
            var lines = text.Split('\n');
            ReplyAsync("A new noun has been selected.").Wait();
            return lines[Program.Randomizer.Next(lines.Length)];
        }
    }
}