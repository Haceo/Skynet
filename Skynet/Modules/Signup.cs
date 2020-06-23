using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skynet.Modules
{
    public class Signup : ModuleBase<SocketCommandContext>
    {
        public static MainWindow _main;
        [Command("signup", RunMode = RunMode.Async)]
        public async Task SignUp(string option, [Remainder]string twitch = "")
        {
            await Task.Delay(200);
            await Context.Channel.DeleteMessageAsync(Context.Message);
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            switch (option.ToLower())
            {
                case "add":
                    Streamer newStreamer = new Streamer()
                    {
                        AutoPost = false,
                        GiveRole = true,
                        DiscordId = Context.User.Id,
                        DiscordName = Context.User.ToString(),
                        TwitchName = twitch,
                        Mention = MentionLevel.None,
                        LastUpdate = "",
                        Streaming = false
                    };
                    server.StreamerList.Add(newStreamer);
                    BotFrame.SaveFile("servers");
                    BotFrame.EmbedWriter(Context.Channel, Context.User,
                        "Skynet",
                        $"Skynet is now watching {Context.User.Mention}", time: -1);
                    break;
                case "remove":
                    var streamRole = Context.Guild.Roles.FirstOrDefault(x => x.Id == server.StreamingRole);
                    var user = Context.Guild.Users.FirstOrDefault(x => x.Id == Context.User.Id);
                    if (user.Roles.Contains(streamRole))
                        user.RemoveRoleAsync(streamRole);
                    var streamer = server.StreamerList.FirstOrDefault(x => x.DiscordId == Context.User.Id);
                    server.StreamerList.Remove(streamer);
                    BotFrame.SaveFile("servers");
                    BotFrame.EmbedWriter(Context.Channel, Context.User,
                        "Skynet",
                        $"Skynet is no longer watching {Context.User.Mention}", time: -1);
                    break;
            }
        }
    }
}
