using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace qtReminder.Modules
{
    public class AdminTools : ModuleBase<SocketCommandContext>
    {
        
        [Command("please")]
        public async Task RenameEveryone([Remainder] string text)
        {
            if (!(Context.User is IGuildUser guildUser)) return;

            var client = Program.ServiceProvider.GetRequiredService<DiscordSocketClient>();

            var roles = guildUser.RoleIds.Select(x => client.GetGuild(guildUser.GuildId).GetRole(x));
            if (!(guildUser.Id == 83677331951976448 || roles.Any(x => x.Name.ToLower() == "passione"))) return;

            var usersInServer = client.GetGuild(guildUser.GuildId).Users;

            foreach (var user in usersInServer)
            {
                try
                {
                    await user.ModifyAsync(x => x.Nickname = text);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}