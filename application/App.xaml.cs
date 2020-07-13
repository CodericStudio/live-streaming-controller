using LiteralLifeChurch.LiveStreamingController.Constants;
using LiteralLifeChurch.LiveStreamingController.Exceptions;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LiteralLifeChurch.LiveStreamingController
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;

            AppCenter.Start(SecretsConstants.AppCenterSecret,
                   typeof(Analytics),
                   typeof(Crashes));

            Analytics.TrackEvent(AnalyticsConstants.AppStart);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        private static void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new NavigationException($"Failed to load page: {e.SourcePageType.FullName}");
        }

        private static void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
