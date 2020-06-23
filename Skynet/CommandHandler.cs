using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Skynet
{
    class CommandHandler
    {
        public static MainWindow _main;
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            _client.JoinedGuild += JoinedGuildHandler;
            _client.ReactionAdded += ReactionAddedHandler;
            _client.MessageReceived += MessageReceivedHandler;
        }

        private async Task JoinedGuildHandler(SocketGuild guild)
        {
            DiscordServer saved = _main.ServerList.FirstOrDefault(x => x.ServerId == guild.Id);
            if (saved != null)
                saved.Active = true;
            else
            {
                DiscordServer newServer = new DiscordServer()
                {
                    Active = true,
                    ServerName = guild.Name,
                    ServerId = guild.Id,
                    Prefix = PrefixChar.None,
                    ServerJoined = DateTime.Now.ToString()
                };
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    _main.ServerList.Add(newServer);
                });
            }
            BotFrame.SaveFile("servers");
            _main.ServerList.Clear();
            BotFrame.LoadFile("servers");
        }
        private async Task MessageReceivedHandler(SocketMessage msg)
        {
            SocketUserMessage message = msg as SocketUserMessage;
            if (message == null)
                return;
            SocketCommandContext context = new SocketCommandContext(_client, message);
            if (context.User.IsBot)
                return;
            var server = _main.ServerList.FirstOrDefault(x => x.ServerId == context.Guild.Id);
            bool admin = false;
            var hasAdminRole = context.Guild.Users.FirstOrDefault(x => x.Id == context.User.Id).Roles.FirstOrDefault(y => y.Id == server.AdminRole);
            if (hasAdminRole != null && hasAdminRole.Id == server.AdminRole)
                admin = true;
            int argPos = 0;
            if (message.HasStringPrefix(((Char)server.Prefix).ToString(), ref argPos)
                || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                if ((server.BotChannel != 0 && msg.Channel.Id == server.BotChannel) || server.BotChannel == 0 || admin)
                {
                    var res = await _service.ExecuteAsync(context, argPos, null);
                    if (!res.IsSuccess && res.Error != CommandError.UnknownCommand)
                    {
                        BotFrame.consoleOut($"Command Handler Error: {res.ErrorReason}");
                        return;
                    }
                    if (!res.IsSuccess && res.Error == CommandError.UnknownCommand)
                    {
                        await Task.Delay(200);
                        await message.DeleteAsync();
                        BotFrame.consoleOut($"Unknown command! Deleted message from user {context.User} in channel {context.Channel.Name}{Environment.NewLine}Message: {message.Content}");
                        return;
                    }
                }
                else
                {
                    BotFrame.consoleOut($"Delete message from user {context.User} in channel {context.Channel.Name}{Environment.NewLine}Message: {context.Message.Content}");
                    return;
                }
            }
        }

        private async Task ReactionAddedHandler(Discord.Cacheable<Discord.IUserMessage, ulong> userMessage, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {
            SocketGuildChannel guildChannel = messageChannel as SocketGuildChannel;
            SocketGuildUser user = guildChannel.Guild.Users.FirstOrDefault(x => x.Id == reaction.UserId);
            DiscordServer server = _main.ServerList.FirstOrDefault(x => x.ServerId == guildChannel.Guild.Id);
            if (server == null || user == null)
                return;
            //Reaction Locks
            foreach (var reactionLock in server.ReactionLockList.Where(x => x.ChannelId == guildChannel.Id && x.MessageId == userMessage.Id))
            {
                if (reactionLock.Emote == reaction.Emote.ToString())
                {
                    var giveRole = guildChannel.Guild.Roles.FirstOrDefault(x => x.Id == reactionLock.GiveRole);
                    var takeRole = guildChannel.Guild.Roles.FirstOrDefault(x => x.Id == reactionLock.TakeRole);
                    if (giveRole == null)
                        return;
                    if (!user.Roles.Contains(giveRole))
                    {
                        BotFrame.consoleOut($"Adding role @{giveRole} to user {user} in server {guildChannel.Guild.Name}");
                        await user.AddRoleAsync(giveRole);
                        if (takeRole != null)
                        {
                            BotFrame.consoleOut($"Removing role @{takeRole} from user {user} in server {guildChannel.Guild.Name}");
                            await user.RemoveRoleAsync(takeRole);
                        }
                    }
                }
            }
            //--------------
        }
    }
}
