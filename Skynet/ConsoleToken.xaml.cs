﻿using System.Windows;

namespace Skynet
{
    public partial class ConsoleToken : Window
    {
        public ConsoleToken()
        {
            InitializeComponent();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (discordTokenBox.Text.Trim() == "0" || discordTokenBox.Text.Trim() == ""
                || twitchTokenBox.Text.Trim() == "0" || twitchTokenBox.Text.Trim() == ""
                || twitchClientIdBox.Text.Trim() == "0" || twitchClientIdBox.Text.Trim() == "")
            {
                MessageBox.Show("You MUST enter all tokens in order for the bot to function, please check all token fields and try again!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
