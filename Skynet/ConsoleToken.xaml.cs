using System.Windows;

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
            DialogResult = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
