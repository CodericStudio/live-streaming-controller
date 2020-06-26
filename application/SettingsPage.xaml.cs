using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LiteralLifeChurch.LiveStreamingController
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButton.IsEnabled = Frame.CanGoBack;
        }

        // region Button Click Handlers

        private void OnBackClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            OnBackRequested();
        }

        private void OnCancelClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            OnBackRequested();
        }

        private void OnSaveClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // TODO later
        }

        // endregion

        // region Helper Methods

        private void OnBackRequested()
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        // endregion
    }
}
