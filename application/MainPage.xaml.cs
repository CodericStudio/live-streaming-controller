using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
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
        private StatusType status = StatusType.NotReady;
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
            PrepareUiForLoading("StreamingWorking", "StatusChecking", Colors.Black);

            viewModel.Status
                .SelectMany(status =>
                {
                    this.status = status.Summary;
                    return viewModel.MapToDisplayableStatus(status);
                })
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(status =>
                {
                    DisplayOutcomeOnUi(status.ButtonTextResource, status.StatusTextResource, status.StatusTextColor);
                }, error =>
                {
                    DisplayOutcomeOnUi("StreamingStart", "StatusNotReady", Colors.Red);
                    status = StatusType.NotReady;
                });
        }

        private void InitializeWindowSize()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(300, 150));
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void StreamButtonClick(object sender, RoutedEventArgs e)
        {
            if (status == StatusType.NotReady)
            {
                PrepareUiForLoading("StreamingWorking", "StatusStarting", Colors.Goldenrod);

                viewModel.StartStreaming
                    .SubscribeOn(NewThreadScheduler.Default)
                    .ObserveOnDispatcher()
                    .Subscribe(outcome =>
                    {
                        DisplayOutcomeOnUi("StreamingStop", "StatusReady", Colors.Green);
                        status = outcome.Summary;
                    }, error =>
                    {
                        DisplayOutcomeOnUi("StreamingStart", "StatusNotReady", Colors.Red);
                        status = StatusType.NotReady;
                    });
            }
            else if (status == StatusType.Ready)
            {
                PrepareUiForLoading("StreamingWorking", "StatusStopping", Colors.Goldenrod);

                viewModel.StopStreaming
                    .SubscribeOn(NewThreadScheduler.Default)
                    .ObserveOnDispatcher()
                    .Subscribe(outcome =>
                    {
                        DisplayOutcomeOnUi("StreamingStart", "StatusNotReady", Colors.Red);
                        status = StatusType.NotReady;
                    }, error =>
                    {
                        status = StatusType.Ready;
                    });
            }
        }

        private void DisplayOutcomeOnUi(string buttonText, string indicatorText, Color indicatorColor)
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            ProgressBar.Visibility = Visibility.Collapsed;
            StreamingButton.Content = loader.GetString(buttonText);
            StreamingButton.IsEnabled = true;
            StatusIndicator.Foreground = new SolidColorBrush(indicatorColor);
            StatusIndicator.Text = loader.GetString(indicatorText);
        }

        private void PrepareUiForLoading(string buttonText, string indicatorText, Color indicatorColor)
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            ProgressBar.Visibility = Visibility.Visible;
            StreamingButton.Content = loader.GetString(buttonText);
            StreamingButton.IsEnabled = false;
            StatusIndicator.Foreground = new SolidColorBrush(indicatorColor);
            StatusIndicator.Text = loader.GetString(indicatorText);
        }
    }
}
