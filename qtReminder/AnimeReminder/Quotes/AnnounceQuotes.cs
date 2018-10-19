using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using qtReminder.AnimeReminder.Models;
using qtReminder.ResponseModule;

namespace qtReminder.AnimeReminder.Quotes
{
    public class AnnounceQuotes
    {
        public static  void AddQuoteWaiter(AnimeGuildModel model)
        {
            ResponseModule.ResponseModule.AddWaiter(new GuildUserWaiter(
                model.Guild, 0, QuoteMessageReceived, WaitsForOwner:false, @params: model, DeleteOnSuccess:false));
        }

        private static async Task<bool> QuoteMessageReceived(SocketMessage socketMessage, object rawmodel)
        {
            if (!(rawmodel is AnimeGuildModel model)) return true; // couldn't add, end waiter and continue.
            if (socketMessage.Channel.Id != model.Channel) return false;
            
            var q = Database.Database.GetQuotes();
            string message = socketMessage.Content;
            if (message.Length > 200) message = message.Substring(0, 200);

            var quoteModel = new QuoteModel();
            quoteModel.Author = socketMessage.Author.Id;
            quoteModel.GuildOrigin = model.Guild;
            quoteModel.Message = message;

            q.Insert(quoteModel);
            
            return true; // could add! go.
        }

        public static Quote GetRandomQuote(ulong guild)
        {
            var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();
            var q = Database.Database.GetQuotes().Find(x => x.GuildOrigin == guild);

            var list = q.ToList();
            if (list.Count == 0) return null;

            Quote quote = new Quote();

            var DatabaseQuote = list[Program.Randomizer.Next(list.Count)];
            var user = client.GetGuild(guild).GetUser(DatabaseQuote.Author);
            
            quote.Message = DatabaseQuote.Message;
            quote.Author = user.Nickname ?? user.Username ?? "???";

            return quote;
        }
    }
}