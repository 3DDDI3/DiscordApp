using Discord.Commands;
using Discord.Interactions;
using DiscordApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordApp.Modules
{
    public class InteractionModule:InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("echo", "Echo an input")]
        public async Task Echo(string input)    
        {
            await RespondAsync(input);
        }
    }
}
