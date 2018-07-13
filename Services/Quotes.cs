using System;
using System.IO;
using System.Linq;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace qtReminder.Nyaa
{
    public class QuoteService
    {
        public const string FILENAME = "quotes.txt";

        private DiscordSocketClient _client;
        
        public QuoteService(IServiceProvider s)
        {
            _client = s.GetRequiredService<DiscordSocketClient>();
        }

        public void AddQuote(ulong userid, ulong guildid, string quote)
        {
            try
            {
                string quoteCutShort = quote.Split('\n')[0];
                string newline =
                    $"{guildid.ToString()} {userid.ToString()} {quoteCutShort.Substring(0, Math.Clamp(quoteCutShort.Length, 0, 100))}\n";
                
                File.AppendAllText(FILENAME, newline);
            }
            catch(Exception) {/**/}
        }

        public Quote GetRandomQuote()
        {
            try
            {
                var lines = File.ReadAllLines(FILENAME);
                string randomLine = lines[Program.Randomizer.Next(0, lines.Length)];
                string[] split = randomLine.Split(' ');
                var guild = _client.GetGuild(ulong.Parse(split[0]));
                var user = guild.GetUser(ulong.Parse(split[1]));
                string username;

                if (user == null) username = "unknown";
                else username = user.Nickname ?? user.Username;

                string quote = string.Join(" ", split, 2, split.Length - 2);

                return new Quote(username, quote);
            }
            catch(Exception) {/**/}

            return default(Quote);
        }

        public struct Quote
        {
            public readonly string Name;
            public readonly string QuoteText;

            public Quote(string name, string quoteText)
            {
                Name = name;
                QuoteText = quoteText;
            }
        }
    }
}