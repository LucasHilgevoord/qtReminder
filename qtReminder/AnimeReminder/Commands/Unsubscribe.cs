using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord.Commands;

namespace qtReminder.AnimeReminder.Commands
{
    public class Unsubscribe : ModuleBase<SocketCommandContext>
    {
        [Command("unsub"), Alias("unsubscribe", "u")]
        public async Task Unsub([Remainder] string text)
        {
            await ReplyAsync("ごめん。。。(´；ω；`) The unsubscribe feature isn't in yet.");
        }
    }
}