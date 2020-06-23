using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Skynet
{
    public partial class ServerManager : Window, INotifyPropertyChanged
    {
        public DiscordSocketClient _client;
        public DiscordServer _server;
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<Streamer> streamerList = new ObservableCollection<Streamer>();
        public ObservableCollection<Streamer> StreamerList
        {
            get { return streamerList; }
            set
            {
                streamerList = value;
                RaisePropertyChanged("StreamerList");
            }
        }
        private ObservableCollection<ReactionLock> reactionLockList = new ObservableCollection<ReactionLock>();
        public ObservableCollection<ReactionLock> ReactionLockList
        {
            get { return reactionLockList; }
            set
            {
                reactionLockList = value;
                RaisePropertyChanged("ReactionLockList");
            }
        }
        public ServerManager()
        {
            InitializeComponent();
            DataContext = this;
            serverPrefixBox.ItemsSource = Enum.GetValues(typeof(PrefixChar));
        }
        private void WPF_Loaded(object sender, RoutedEventArgs e)
        {
            //Load
            //Streamers
            if (_server.StreamerList != null)
                foreach (var streamer in _server.StreamerList)
                    StreamerList.Add(streamer);
            //ReactionLocks
            if (_server.ReactionLockList != null)
                foreach (var reactionLock in _server.ReactionLockList)
                    ReactionLockList.Add(reactionLock);
            //Settings
            if (_server.Prefix != PrefixChar.None)
            {
                serverPrefixCheckBox.IsChecked = true;
                serverPrefixBox.SelectedValue = _server.Prefix;
            }
            adminRoleBox.Text = _server.AdminRole.ToString();
            botChanBox.Text = _server.BotChannel.ToString();
            newUserRoleBox.Text = _server.NewUserRole.ToString();
            intervalSlider.Value = _server.StreamerCheckInterval;
            streamPostChannelBox.Text = _server.StreamPostChannel.ToString();
            streamerRoleBox.Text = _server.StreamingRole.ToString();
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Global
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //Save
            //Streamers
            //ReactionLocks
            //Settings
            if (serverPrefixCheckBox.IsChecked == true)
                _server.Prefix = (PrefixChar)serverPrefixBox.SelectedItem;
            if (ulong.TryParse(adminRoleBox.Text, out ulong adminRole))
                _server.AdminRole = adminRole;
            else
                _server.AdminRole = 0;
            if (ulong.TryParse(botChanBox.Text, out ulong botChan))
                _server.BotChannel = botChan;
            else
                _server.BotChannel = 0;
            if (ulong.TryParse(newUserRoleBox.Text, out ulong newUserRole))
                _server.NewUserRole = newUserRole;
            else
                _server.NewUserRole = 0;
            _server.StreamerCheckInterval = intervalSlider.Value;
            if (ulong.TryParse(streamPostChannelBox.Text, out ulong postChannel))
                _server.StreamPostChannel = postChannel;
            else
                _server.StreamPostChannel = 0;
            if (ulong.TryParse(streamerRoleBox.Text, out ulong streamerRole))
                _server.StreamingRole = streamerRole;
            else
                _server.StreamingRole = 0;
            BotFrame.SaveFile("servers");
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //Streamers
        private void StreamersAdd_Click(object sender, RoutedEventArgs e)
        {
            StreamerEditor se = new StreamerEditor();
            se.Owner = this;
            se.Title = "New Streamer";
            se.mentionLevelComboBox.SelectedItem = MentionLevel.None;
            se.ShowDialog();
            if (se.DialogResult.HasValue && se.DialogResult.Value)
            {
                Streamer newStreamer = new Streamer()
                {
                    Streaming = false,
                    DiscordId = ulong.Parse(se.discordIdBox.Text),
                    DiscordName = _client.Guilds.FirstOrDefault(x => x.Id == _server.ServerId).Users.FirstOrDefault(y => y.Id == ulong.Parse(se.discordIdBox.Text)).ToString(),
                    Mention = (MentionLevel)se.mentionLevelComboBox.SelectedItem,
                    GiveRole = se.giveRoleCheckBox.IsChecked.Value,
                    AutoPost = se.autoPostCheckBox.IsChecked.Value,
                    TwitchName = se.twitchNameBox.Text
                };
                if (_server.StreamerList == null)
                    _server.StreamerList = new List<Streamer>();
                _server.StreamerList.Add(newStreamer);
                UpdateView("streamer");
                BotFrame.SaveFile("servers");
            }
        }
        private void StreamersEdit_Click(object sender, RoutedEventArgs e)
        {
            StreamerEditor se = new StreamerEditor();
            se.Owner = this;
            se._streamer = _server.StreamerList[streamerListBox.SelectedIndex];
            se.Title = $"Edit Streamer {se._streamer.DiscordName}";
            se.ShowDialog();
            if (se.DialogResult.HasValue && se.DialogResult.Value)
            {
                se._streamer.DiscordId = ulong.Parse(se.discordIdBox.Text);
                se._streamer.DiscordName = _client.Guilds.FirstOrDefault(x => x.Id == _server.ServerId).Users.FirstOrDefault(y => y.Id == ulong.Parse(se.discordIdBox.Text)).ToString();
                se._streamer.TwitchName = se.twitchNameBox.Text;
                se._streamer.Mention = (MentionLevel)se.mentionLevelComboBox.SelectedItem;
                se._streamer.GiveRole = se.giveRoleCheckBox.IsChecked.Value;
                se._streamer.AutoPost = se.autoPostCheckBox.IsChecked.Value;
                UpdateView("streamers");
                BotFrame.SaveFile("servers");
            }
        }
        private void StreamersDelete_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to remove this item?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No)
                return;
            _server.StreamerList.Remove(_server.StreamerList[streamerListBox.SelectedIndex]);
            UpdateView("streamers");
            BotFrame.SaveFile("servers");
        }
        //ReactionLocks
        private void ReactionLocksAdd_Click(object sender, RoutedEventArgs e)
        {
            ReactionLockEditor rl = new ReactionLockEditor();
            rl.Owner = this;
            rl.Title = "New Reaction Lock";
            rl.ShowDialog();
            if (rl.DialogResult.HasValue && rl.DialogResult.Value)
            {
                ReactionLock newLock = new ReactionLock()
                {
                    ChannelId = ulong.Parse(rl.channelBox.Text),
                    MessageId = ulong.Parse(rl.messageBox.Text),
                    Emote = rl.emoteBox.Text,
                    GiveRole = ulong.Parse(rl.giveRoleBox.Text),
                    TakeRole = ulong.Parse(rl.takeRoleBox.Text)
                };
                if (_server.ReactionLockList == null)
                    _server.ReactionLockList = new List<ReactionLock>();
                _server.ReactionLockList.Add(newLock);
                UpdateView("reactionlock");
                BotFrame.SaveFile("servers");
            }
        }
        private void ReactionLocksEdit_Click(object sender, RoutedEventArgs e)
        {
            ReactionLockEditor rl = new ReactionLockEditor();
            rl.Owner = this;
            rl._lock = _server.ReactionLockList[reactionListBox.SelectedIndex];
            rl.Title = $"Edit lock {rl._lock.MessageId}";
            rl.ShowDialog();
            if (rl.DialogResult.HasValue && rl.DialogResult.Value)
            {
                rl._lock.ChannelId = ulong.Parse(rl.channelBox.Text);
                rl._lock.MessageId = ulong.Parse(rl.messageBox.Text);
                rl._lock.Emote = rl.emoteBox.Text;
                rl._lock.GiveRole = ulong.Parse(rl.giveRoleBox.Text);
                rl._lock.TakeRole = ulong.Parse(rl.takeRoleBox.Text);
                UpdateView("reactionlock");
                BotFrame.SaveFile("servers");
            }
        }
        private void ReactionLocksDelete_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure you want to remove this item?", "Delete?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No)
                return;
            _server.ReactionLockList.Remove(_server.ReactionLockList[reactionListBox.SelectedIndex]);
            UpdateView("reactionlock");
            BotFrame.SaveFile("servers");
        }
        //Settings
        private void Prefix_Enable(object sender, RoutedEventArgs e)
        {
            serverPrefixBox.IsEnabled = true;
        }
        private void Prefix_Disable(object sender, RoutedEventArgs e)
        {
            serverPrefixBox.IsEnabled = false;
            serverPrefixBox.SelectedItem = PrefixChar.None;
        }

        private void UpdateView(string view)
        {
            switch (view)
            {
                case "streamers":
                    StreamerList.Clear();
                    foreach (var streamer in _server.StreamerList)
                        StreamerList.Add(streamer);
                    break;
                case "reactionlock":
                    ReactionLockList.Clear();
                    foreach (var reactionLock in _server.ReactionLockList)
                        ReactionLockList.Add(reactionLock);
                    break;
            }
        }
    }
}
