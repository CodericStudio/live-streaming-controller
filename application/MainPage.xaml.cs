using Windows.UI.Xaml.Controls;

namespace LiteralLifeChurch.LiveStreamingController
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void GoToSettings(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
