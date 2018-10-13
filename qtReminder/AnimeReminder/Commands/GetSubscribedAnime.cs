using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace qtReminder.AnimeReminder.Commands
{
    public class GetSubscribedAnime : ModuleBase<SocketCommandContext>
    {
        [Command("subs"), Alias("subscriptions")]
        public async Task GetAnime()
        {
            var d = Database.Database.GetDatabaseAndSubscriptionCollection();
            // For some godforsaken reason SubscribedUsers does some weird stuff when it is not
            // converted to a list. So that's what I'm doing here. Don't look at me weird if you
            // think it's odd that im doing this.
            var subbedAnimeCollection = d.collection.Find(x => x.Guild == Context.Guild.Id
                                                               && x.SubscribedUsers.ToList().Contains(Context.User.Id)).ToList();
            
            var stringBuilder = new StringBuilder("Your subscribed anime:\n");

            foreach (var s in subbedAnimeCollection)
            {
                stringBuilder.AppendLine(s.AnimeTitle.EnglishTitle ?? s.AnimeTitle.RomajiTitle);
            }

            await ReplyAsync(stringBuilder.ToString());
        }
    }
}