using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;

namespace Skynet.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        public TwitchAPI _api = new TwitchAPI();
        [Command("emoteid", RunMode = RunMode.Async)]
        public async Task GetEmoteId(string emote)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            BotFrame.consoleOut($"{emote}");
        }
    }
}
