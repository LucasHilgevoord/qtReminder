using System.Threading.Tasks;
using Discord.Commands;
using qtReminder.AnimeReminder.Services;

namespace qtReminder.AnimeReminder.Commands
{
    public class ForceCheck : ModuleBase<SocketCommandContext>
    {
        [Command("force check")]
        public async Task ForceCheckCommand()
        {
            await AnimeReminderHandler.Check();
        }
    }
}