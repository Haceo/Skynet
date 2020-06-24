using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skynet.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("norole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task NoRole()
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            string outNone = "";
            foreach (var user in Context.Guild.Users.Where(x => x.Roles.Count == 1))
                outNone += $"{user.Mention} Joined: {user.JoinedAt}";
            if (outNone == "")
            {
                await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Skynet Admin",
                    $"Sorry no users found without any roles.");
                return;
            }
            await BotFrame.EmbedWriter(Context.Channel, Context.User,
                    "Skynet Admin",
                    $"Users found with no role:{Environment.NewLine}{outNone}");
        }
    }
}
