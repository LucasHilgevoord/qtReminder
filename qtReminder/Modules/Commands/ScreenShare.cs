using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace qtReminder.Modules.Commands
{
    public class ScreenShare : ModuleBase<SocketCommandContext>
    {
        [Command("share"),
        Remarks("Screen sharing is now possible in a guild")]
        public async Task PostScreenShare()
        {
            // if not in a guild, whine.
            if (!(Context.Channel is IGuildChannel channel))
            {
                await ReplyAsync($"{Context.User.Mention}, you need to be in a guild to use this command!");
                return;
            }

            var guildUser = Context.User as IGuildUser;

            if (guildUser.VoiceChannel == null)
            {
                await ReplyAsync($"{Context.User.Mention}, get into a voice channel first!");
                return;
            }

            await ReplyAsync($"{guildUser.Mention} to enable screen sharing, click this link:\n" +
                             $"<https://discordapp.com/channels/{guildUser.GuildId}/{guildUser.VoiceChannel.Id}>");
        }
    }
}