using LiteralLifeChurch.LiveStreamingController.Models;
using LiteralLifeChurch.LiveStreamingController.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LiteralLifeChurch.LiveStreamingController
{
    public sealed partial class SettingsPage : Page
    {
        private readonly SettingsService settings;

        public SettingsPage()
        {
            settings = new SettingsService();

            InitializeComponent();
            InitializeForm();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BackButton.IsEnabled = Frame.CanGoBack;
        }

        // region Button Click Handlers

        private void OnApiKeyChanged(object sender, RoutedEventArgs e)
        {
            EnableSaveButtonIfFormIsValid();
        }

        private async void OnBackClick(object sender, RoutedEventArgs e)
        {
            await CheckFormBeforeNavigating();
        }

        private async void OnCancelClick(object sender, RoutedEventArgs e)
        {
            await CheckFormBeforeNavigating();
        }

        private void OnHostChanged(object sender, TextChangedEventArgs e)
        {
            EnableSaveButtonIfFormIsValid();
        }

        private void OnLiveEventsChanged(object sender, TextChangedEventArgs e)
        {
            EnableSaveButtonIfFormIsValid();
        }

        private void OnStreamingEndpointChanged(object sender, TextChangedEventArgs e)
        {
            EnableSaveButtonIfFormIsValid();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            settings.Storage = new SettingsModel
            {
                ApiKey = ApiKey.Password,
                Host = Host.Text,
                LiveEventNames = LiveEvents.Text,
                StreamingEndpointName = StreamingEndpoint.Text
            };

            OnBackRequested();
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

        private async Task CheckFormBeforeNavigating()
        {
            SettingsModel model = settings.Storage;
            bool isApiKeySame = ApiKey.Password == model.ApiKey;
            bool isHostSame = Host.Text == model.Host;
            bool isLiveEventsSame = LiveEvents.Text == model.LiveEventNames;
            bool isStreamingEndpointSame = StreamingEndpoint.Text == model.StreamingEndpointName;

            if (isApiKeySame && isHostSame && isLiveEventsSame && isStreamingEndpointSame)
            {
                OnBackRequested();
            }
            else
            {
                await ShowDialog();
            }
        }

        private void EnableSaveButtonIfFormIsValid()
        {
            bool isApiKeyValid = !string.IsNullOrWhiteSpace(ApiKey.Password);
            bool isHostValid = !string.IsNullOrWhiteSpace(Host.Text) && Uri.IsWellFormedUriString($"https://{Host.Text}/", UriKind.Absolute);
            bool isLiveEventsValid = !string.IsNullOrWhiteSpace(LiveEvents.Text);
            bool isStreamingEndpointValid = !string.IsNullOrWhiteSpace(StreamingEndpoint.Text);

            SaveButton.IsEnabled = isApiKeyValid && isHostValid && isLiveEventsValid && isStreamingEndpointValid;
        }

        private void InitializeForm()
        {
            SettingsModel model = settings.Storage;
            ApiKey.Password = model.ApiKey;
            Host.Text = model.Host;
            LiveEvents.Text = model.LiveEventNames;
            StreamingEndpoint.Text = model.StreamingEndpointName;

            EnableSaveButtonIfFormIsValid();
        }

        private async Task ShowDialog()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();
            ContentDialog dialog = new ContentDialog
            {
                Title = loader.GetString("DialogTitle"),
                Content = loader.GetString("DialogContent"),
                CloseButtonText = loader.GetString("DialogClose"),
                PrimaryButtonText = loader.GetString("DialogPrimary")
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                OnBackRequested();
            }
        }

        // endregion
    }
}
