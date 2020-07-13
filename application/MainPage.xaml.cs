using esgeeFlatToggleSwitch;
using LiteralLifeChurch.LiveStreamingController.Constants;
using LiteralLifeChurch.LiveStreamingController.Enums;
using LiteralLifeChurch.LiveStreamingController.Models;
using LiteralLifeChurch.LiveStreamingController.Services;
using Microsoft.AppCenter.Analytics;
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
        private readonly NotificationService Notifications;
        private readonly ResourceLoader ResourceLoader;
        private readonly SettingsService Settings;

        private static readonly double SwitchOpaque = 1.0;
        private static readonly double SwitchTranspatent = 0.3;

        IDisposable HeartBeatHandle;
        private bool IsSwitchOn;
        private bool WasSwitchedByProgram = true;

        public MainPage()
        {
            InitializeComponent();
            Analytics.TrackEvent(AnalyticsConstants.ViewedMainScreen);

            Api = new ApiService();
            Notifications = new NotificationService();
            ResourceLoader = ResourceLoader.GetForCurrentView();
            Settings = new SettingsService();

            IsSwitchOn = ToggleSwitch.IsOn;
            SetupUi();
            InitializeHeartBeat();
        }

        private void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent(AnalyticsConstants.ManualStatusRefresh);
            FetchStatus();
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            if (HeartBeatHandle != null)
            {
                HeartBeatHandle.Dispose();
            }

            Frame.Navigate(typeof(SettingsPage));
        }

        private void OnStopAllClick(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent(AnalyticsConstants.ServicesForcefullyStopped);

            if (HeartBeatHandle != null)
            {
                HeartBeatHandle.Dispose();
            }

            SetSwitchState(false);
            Spinner.Visibility = Visibility.Visible;
            ToggleSwitch.IsEnabled = false;
            ToggleSwitch.Opacity = SwitchTranspatent;
            StatusText.Text = ResourceLoader.GetString("StatusStopping");
            MonitorObservable(Api.Stop(), true, true);
        }

        private void OnSwitchToggle(object sender, RoutedEventArgs e)
        {
            FlatToggleSwitch toggle = sender as FlatToggleSwitch;

            if (WasSwitchedByProgram)
            {
                WasSwitchedByProgram = false;
                return;
            }

            if (HeartBeatHandle != null)
            {
                HeartBeatHandle.Dispose();
            }

            Spinner.Visibility = Visibility.Visible;
            ToggleSwitch.IsEnabled = false;
            ToggleSwitch.Opacity = SwitchTranspatent;

            if (toggle.IsOn)
            {
                StatusText.Text = ResourceLoader.GetString("StatusStarting");
                Analytics.TrackEvent(AnalyticsConstants.ServicesStarted);
                MonitorObservable(Api.Start(), true, true);
            }
            else
            {
                StatusText.Text = ResourceLoader.GetString("StatusStopping");
                Analytics.TrackEvent(AnalyticsConstants.ServicesStopped);
                MonitorObservable(Api.Stop(), true, true);
            }
        }

        // region Helper Methods

        private void FetchStatus()
        {
            Spinner.Visibility = Visibility.Visible;
            StatusText.Text = ResourceLoader.GetString("StatusChecking");
            ToggleSwitch.IsEnabled = false;
            ToggleSwitch.Opacity = SwitchTranspatent;

            MonitorObservable(Api.GetStatusUntilStable(), false, false);
        }

        private NotificationContentModel GenerateNotificationText(ResourceStatusEnum status)
        {
            switch (status)
            {
                case ResourceStatusEnum.Running:
                    return new NotificationContentModel
                    {
                        Message = ResourceLoader.GetString("StatusRunning"),
                        Title = ResourceLoader.GetString("ServicesOn")
                    };

                case ResourceStatusEnum.Stopped:
                    return new NotificationContentModel
                    {
                        Message = ResourceLoader.GetString("StatusStopped"),
                        Title = ResourceLoader.GetString("ServicesOff")
                    };

                default:
                    return new NotificationContentModel
                    {
                        Message = ResourceLoader.GetString("StatusError"),
                        Title = ResourceLoader.GetString("ServicesError")
                    };
            }
        }

        private void InitializeHeartBeat()
        {
            if (!Settings.AreSettingsPopulated || !Settings.Storage.PollingIntervalEnabled)
            {
                return;
            }

            HeartBeatHandle = Observable
                .Interval(TimeSpan.FromMinutes(Settings.Storage.PollingInterval))
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(time =>
                {
                    FetchStatus();
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

        private void MonitorObservable(IObservable<StatusModel> observable, bool showNotification, bool restartHeartBeat)
        {
            observable
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(outcome =>
                {
                    StatusText.Text = MapStatus(outcome.Summary.Name);
                    SetSwitchState(outcome.Summary.Name == ResourceStatusEnum.Running);

                    if (showNotification)
                    {
                        Notifications.ShowNotification(GenerateNotificationText(outcome.Summary.Name));
                    }

                    if (restartHeartBeat)
                    {
                        InitializeHeartBeat();
                    }
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

        // Used to bypass a stange animation which happens whenever you
        // change the switch state to the same state it was already in
        private void SetSwitchState(bool isOn)
        {
            if (isOn == IsSwitchOn)
            {
                return;
            }

            IsSwitchOn = isOn;
            WasSwitchedByProgram = true;

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
