using LiteralLifeChurch.LiveStreamingController.ViewModels;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LiteralLifeChurch.LiveStreamingController
{
    public sealed partial class MainPage : Page
    {
        private MainWindowViewModel viewModel = new MainWindowViewModel();

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
            ResourceLoader loader = ResourceLoader.GetForCurrentView();
            StatusIndicator.Foreground = new SolidColorBrush(Colors.Black);
            StatusIndicator.Text = loader.GetString("StatusChecking");

            viewModel.Status
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(status =>
                {
                    StatusIndicator.Foreground = new SolidColorBrush(status.StatusTextColor);
                    StatusIndicator.Text = loader.GetString(status.StatusTextResource);
                }, error =>
                {
                    StatusIndicator.Foreground = new SolidColorBrush(Colors.Red);
                    StatusIndicator.Text = loader.GetString("StatusNotReady");
                });
        }

        private void InitializeWindowSize()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(300, 150));
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }
    }
}
