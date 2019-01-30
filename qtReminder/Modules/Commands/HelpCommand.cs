using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace qtReminder.Modules.Commands
{
    public class HelpCommand : ModuleBase<CommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            await this.ReplyAsync("no");
        }

        [Command("commands"),
        Remarks("Displays this")]
        public async Task ListCommands()
        {
            var commandService = Program.ServiceProvider.GetRequiredService<CommandService>();
            
            var commands = new StringBuilder("```");

            foreach (var command in commandService.Commands)
            {
                commands.AppendLine($"${command.Name} \t {command.Remarks}");
            }

            commands.AppendLine("```");

            await this.ReplyAsync(commands.ToString());
        }
    }
}