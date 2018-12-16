using LiteralLifeChurch.LiveStreamingController.Services.Firebase;
using LiteralLifeChurch.LiveStreamingController.ViewModels;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LiteralLifeChurch.LiveStreamingController
{
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel viewModel = new MainPageViewModel();

        public MainPage()
        {
            InitializeComponent();
            InitializeWindowSize();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            InitializeStatus();
        }

        private void InitializeStatus()
        {
            PrepareUiForLoading("StatusChecking", Colors.Black);

            viewModel.Status
                .SelectMany(status =>
                {
                    viewModel.CacheServices(status);
                    return Observable.Return(status);
                })
                .SelectMany(status =>
                {
                    return viewModel.MapToDisplayableStatus(status);
                })
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(status =>
                {
                    DisplayOutcomeOnUi(status.StatusTextResource, status.StatusTextColor);
                }, error =>
                {
                    DisplayOutcomeOnUi("StatusNotReady", Colors.Red);
                });
        }

        private void InitializeWindowSize()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(300, 150));
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void StreamButtonClick(object sender, RoutedEventArgs e)
        {
            PrepareUiForLoading("StatusStarting", Colors.Goldenrod);

            viewModel.StartStreaming
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(outcome =>
                {
                    DisplayOutcomeOnUi("StatusReady", Colors.Green);
                }, error =>
                {
                    DisplayOutcomeOnUi("StatusNotReady", Colors.Red);
                });
        }

        private void DisplayOutcomeOnUi(string indicatorText, Color indicatorColor)
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            ProgressBar.Visibility = Visibility.Collapsed;
            StreamingButton.IsEnabled = true;
            StatusIndicator.Foreground = new SolidColorBrush(indicatorColor);
            StatusIndicator.Text = loader.GetString(indicatorText);
        }

        private void PrepareUiForLoading(string indicatorText, Color indicatorColor)
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            ProgressBar.Visibility = Visibility.Visible;
            StreamingButton.IsEnabled = false;
            StatusIndicator.Foreground = new SolidColorBrush(indicatorColor);
            StatusIndicator.Text = loader.GetString(indicatorText);
        }
    }
}
