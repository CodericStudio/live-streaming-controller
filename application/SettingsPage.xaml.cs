using LiteralLifeChurch.LiveStreamingController.Models;
using LiteralLifeChurch.LiveStreamingController.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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

        private void OnPollingIntervalEnabledChecked(object sender, RoutedEventArgs e)
        {
            PollingInterval.IsEnabled = true;
            EnableSaveButtonIfFormIsValid();
        }

        private void OnPollingIntervalEnabledUnchecked(object sender, RoutedEventArgs e)
        {
            PollingInterval.IsEnabled = false;
            EnableSaveButtonIfFormIsValid();
        }

        private void OnPollingIntervalChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            EnableSaveButtonIfFormIsValid();
        }

        private void OnPollingIntervalWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            EnableSaveButtonIfFormIsValid();
        }

        private void OnStreamingEndpointChanged(object sender, TextChangedEventArgs e)
        {
            EnableSaveButtonIfFormIsValid();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            int interval;

            try
            {
                interval = Convert.ToInt32(PollingInterval.Text);
            }
            catch (Exception)
            {
                interval = 1;
            }

            settings.Storage = new SettingsModel
            {
                ApiKey = ApiKey.Password,
                Host = Host.Text,
                LiveEventNames = LiveEvents.Text,
                PollingInterval = interval,
                PollingIntervalEnabled = PollingIntervalEnabled.IsChecked == true,
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
            bool isPollingIntervalSame = PollingInterval.Text == model.PollingInterval.ToString();
            bool isPollingIntervalEnabledSame = PollingIntervalEnabled.IsChecked == model.PollingIntervalEnabled;
            bool isStreamingEndpointSame = StreamingEndpoint.Text == model.StreamingEndpointName;

            bool isFormSame = isApiKeySame && isHostSame && isLiveEventsSame;
            isFormSame &= isPollingIntervalSame && isPollingIntervalEnabledSame && isStreamingEndpointSame;

            if (isFormSame)
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
            bool isIntervalValid = true;
            bool isStreamingEndpointValid = !string.IsNullOrWhiteSpace(StreamingEndpoint.Text);

            if (PollingIntervalEnabled.IsChecked == true)
            {
                double value = PollingInterval.Value;
                isIntervalValid = 1.0 <= value && value <= 1000.0;
            }

            SaveButton.IsEnabled = isApiKeyValid && isHostValid && isLiveEventsValid && isIntervalValid && isStreamingEndpointValid;
        }

        private void InitializeForm()
        {
            SettingsModel model = settings.Storage;
            ApiKey.Password = model.ApiKey;
            Host.Text = model.Host;
            LiveEvents.Text = model.LiveEventNames;
            PollingInterval.IsEnabled = model.PollingIntervalEnabled;
            PollingInterval.Text = model.PollingInterval.ToString();
            PollingIntervalEnabled.IsChecked = model.PollingIntervalEnabled;
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
