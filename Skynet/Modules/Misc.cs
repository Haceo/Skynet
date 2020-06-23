using Discord.Commands;
using System.Threading.Tasks;

namespace Skynet.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("emoteid", RunMode = RunMode.Async)]
        public async Task GetEmoteId(string emote)
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            BotFrame.consoleOut($"{emote}");
        }
    }
}
