using System.Threading.Tasks;
using Discord.Commands;

namespace qtReminder.Modules
{
    public class StringSimilarityModule : ModuleBase<SocketCommandContext>
    {
        [Command("sim")]
        public async Task CheckSimilarity([Remainder] string t)
        {
            string[] split = t.Split('|');
            if (split.Length != 2) return;
            
            double c = split[0].GetSimilarity(split[1]);
            await ReplyAsync($"{Context.User.Mention}: {c}");
        }
    }
}