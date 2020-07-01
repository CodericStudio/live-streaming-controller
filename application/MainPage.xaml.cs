using LiteralLifeChurch.LiveStreamingController.Enums;
using LiteralLifeChurch.LiveStreamingController.Services;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LiteralLifeChurch.LiveStreamingController
{
    public sealed partial class MainPage : Page
    {
        private readonly ApiService Api;
        private readonly ResourceLoader ResourceLoader;
        private readonly SettingsService Settings;

        private static readonly double SwitchOpaque = 1.0;
        private static readonly double SwitchTranspatent = 0.3;

        private bool IsSwitchOn;

        public MainPage()
        {
            InitializeComponent();

            Api = new ApiService();
            ResourceLoader = ResourceLoader.GetForCurrentView();
            Settings = new SettingsService();

            IsSwitchOn = ToggleSwitch.IsOn;
            SetupUi();
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            FetchStatus();
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        // region Helper Methods

        private void FetchStatus()
        {
            Spinner.Visibility = Visibility.Visible;
            StatusText.Text = ResourceLoader.GetString("StatusChecking");
            ToggleSwitch.IsEnabled = false;
            ToggleSwitch.Opacity = SwitchTranspatent;

            Api.GetStatusUntilStable()
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(status =>
                {
                    StatusText.Text = MapStatus(status.Summary.Name);
                    SetSwitchState(status.Summary.Name == ResourceStatusEnum.Running);
                }, error =>
                {
                    Spinner.Visibility = Visibility.Collapsed;
                    SetSwitchState(false);
                    StatusText.Text = ResourceLoader.GetString("NetworkError");
                }, () =>
                {
                    Spinner.Visibility = Visibility.Collapsed;
                    ToggleSwitch.IsEnabled = true;
                    ToggleSwitch.Opacity = SwitchOpaque;
                });
        }

        private string MapStatus(ResourceStatusEnum status)
        {
            switch (status)
            {
                case ResourceStatusEnum.Running:
                    return ResourceLoader.GetString("StatusRunning");

                case ResourceStatusEnum.Starting:
                    return ResourceLoader.GetString("StatusStarting");

                case ResourceStatusEnum.Stopped:
                    return ResourceLoader.GetString("StatusStopped");

                case ResourceStatusEnum.Stopping:
                    return ResourceLoader.GetString("StatusStopping");

                default:
                    return ResourceLoader.GetString("StatusError");
            }
        }

        private void SetSwitchState(bool isOn)
        {
            if (isOn == IsSwitchOn)
            {
                return;
            }

            IsSwitchOn = isOn;
            ToggleSwitch.IsOn = isOn;
        }

        private void SetupUi()
        {
            if (Settings.AreSettingsPopulated)
            {
                FetchStatus();
            }
            else
            {
                SetSwitchState(false);
                Spinner.Visibility = Visibility.Collapsed;
                StatusText.Text = ResourceLoader.GetString("NotSetUp");
                ToggleSwitch.IsEnabled = false;
                ToggleSwitch.Opacity = SwitchTranspatent;
            }
        }

        // endregion
    }
}
