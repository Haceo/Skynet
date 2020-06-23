using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Skynet
{
    class BotFrame
    {
        private const string resDir = "Resources";
        public static MainWindow _main;
        public static Config config;
        public static bool TimeStamp = true;

        public static async Task LoadFile(string file)
        {
            _main.loading = true;
            if (!Check(file))
            {
                consoleOut($"File {file} not found!");
                _main.loading = false;
                return;
            }
            else
            {
                consoleOut($"File {file} found! Loading...");
                string json = File.ReadAllText($"{resDir}/{file}.json");
                switch (file)
                {
                    case "config":
                        config = new Config();
                        config = JsonConvert.DeserializeObject<Config>(json);
                        consoleOut("Config loaded!");
                        break;
                    case "servers":
                        _main.ServerList = new ObservableCollection<DiscordServer>();
                        _main.ServerList = JsonConvert.DeserializeObject<ObservableCollection<DiscordServer>>(json);
                        consoleOut("Servers loaded!");
                        break;
                }
            }
            _main.loading = false;
        }
        public static async Task SaveFile(string file)
        {
            consoleOut($"Saving {file}...");
            string json = "";
            switch (file)
            {
                case "config":
                    json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    break;
                case "servers":
                    json = JsonConvert.SerializeObject(_main.ServerList, Formatting.Indented);
                    break;
            }
            try
            {
                File.WriteAllText($"{resDir}/{file}.json", json);
            }
            catch (Exception ex)
            {
                BotFrame.consoleOut(ex.Message);
                return;
            }
            consoleOut($"Saved {file}!");
        }
        public static bool Check(string file)
        {
            if (!Directory.Exists(resDir))
                Directory.CreateDirectory(resDir);
            if (!File.Exists($"{resDir}/{file}.json"))
                return false;
            else
                return true;
        }
        public static void consoleOut(string msg)
        {
            string timeNow = "";
            if (TimeStamp)
                timeNow = $"{DateTime.Now}: ";
            _main.ConsoleString += $"{timeNow}{msg}{Environment.NewLine}";
        }
        public static void consoleClear()
        {
            string timeNow = "";
            if (TimeStamp)
                timeNow = $"{DateTime.Now}: ";
            _main.ConsoleString = $"{timeNow}I was just cleared!{Environment.NewLine}";
        }
        public static async Task<ulong> EmbedWriter(ISocketMessageChannel chan, IUser user, string title, string data, bool pinned = false, bool image = true, bool direct = false, int time = 30000)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(title);
            embed.WithUrl("https://github.com/Haceo/SkynetBot");
            embed.WithDescription(data);
            embed.WithColor(new Color(100, 65, 165));
            embed.WithFooter($"Author: {user} - {user.Id}");
            if (image)
                embed.WithThumbnailUrl(user.GetAvatarUrl());
            var embedded = embed.Build();
            if (time == -1 && !direct)
            {
                var msg = await chan.SendMessageAsync("", false, embedded);
                if (pinned)
                    await msg.PinAsync();
                return msg.Id;
            }
            else
            {
                if (!direct)
                {
                    var msg = await chan.SendMessageAsync("", false, embedded);
                    await Task.Delay(time);
                    await chan.DeleteMessageAsync(msg);
                    return msg.Id;
                }
                else
                {
                    var msg = await user.SendMessageAsync("", false, embedded);
                    return msg.Id;
                }
            }
        }
        public static async Task StreamPost(ISocketMessageChannel chan, IUser user, TwitchLib.Api.V5.Models.Streams.Stream stream, int loud = 0)
        {
            var url = stream.Channel.Url;
            var embed = new EmbedBuilder();
            embed.WithAuthor(new EmbedAuthorBuilder() { Name = $"{user.Username} is streaming on Twitch!", IconUrl = user.GetAvatarUrl(), Url = url });
            embed.WithTitle(stream.Channel.Status);
            embed.WithUrl(url);
            embed.WithColor(new Color(100, 65, 165));
            embed.AddField($"Playing {stream.Channel.Game} for {stream.Viewers} viewers!", $"[Watch Stream]({url})");
            embed.WithImageUrl(stream.Preview.Large + $"?time={(Int32)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds}");
            embed.WithFooter("Skynet is always watching!");
            var embeded = embed.Build();
            string mention = "";
            switch (loud)
            {
                case 0:
                    mention = "";
                    break;
                case 1:
                    mention = "@here";
                    break;
                case 2:
                    mention = "@everyone";
                    break;
            }
            await chan.SendMessageAsync(mention, false, embeded);
        }
    }
    public class Config
    {
        public string DiscordToken { get; set; }
        public string TwitchToken { get; set; }
        public string TwitchClientId { get; set; }
    }
    public enum PrefixChar
    {
        None,
        Exclamation = '!',
        At = '@',
        Pound = '#',
        Dollar = '$',
        Percent = '%',
        Carrot = '^',
        Ampersand = '&',
        Astrisk = '*'
    }
    public class DiscordServer
    {
        public bool Active { get; set; }
        public string ServerName { get; set; }
        public ulong ServerId { get; set; }
        public ulong AdminRole { get; set; }
        public ulong NewUserRole { get; set; }
        public ulong BotChannel { get; set; }
        public PrefixChar Prefix { get; set; }
        public string ServerJoined { get; set; }
        public ulong StreamPostChannel { get; set; }
        public List<Streamer> StreamerList { get; set; }
        public ulong StreamingRole { get; set; }
        public double StreamerCheckInterval { get; set; }
        public double StreamerCheckElapsed { get; set; }
        public List<ReactionLock> ReactionLockList { get; set; }
    }
    public enum MentionLevel
    {
        None = 0,
        Here = 1,
        Everyone = 2
    }
    public class Streamer
    {
        public bool Streaming { get; set; }
        public string DiscordName { get; set; }
        public ulong DiscordId { get; set; }
        public string TwitchName { get; set; }
        public string Game { get; set; }
        public bool GiveRole { get; set; }
        public bool AutoPost { get; set; }
        public MentionLevel Mention { get; set; }
    }
    public class ReactionLock
    {
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public string Emote { get; set; }
        public ulong GiveRole { get; set; }
        public ulong TakeRole { get; set; }
    }
}
