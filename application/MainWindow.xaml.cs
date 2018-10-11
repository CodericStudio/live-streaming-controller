using LiteralLifeChurch.LiveStreamingController.Resources;
using LiteralLifeChurch.LiveStreamingController.ViewModels;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace LiteralLifeChurch.LiveStreamingController
{
    public partial class MainWindow : Window
    {

        private MainWindowViewModel viewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitializeStatus();
        }

        private void InitializeStatus()
        {
            statusIndicator.Content = Strings.StatusChecking;
            statusIndicator.Foreground = Brushes.Black;

            viewModel.GetStatus()
                .SubscribeOn(NewThreadScheduler.Default)
                .ObserveOn(DispatcherScheduler.Current)
                .Subscribe(status =>
                {
                    statusIndicator.Content = status.StatusText;
                    statusIndicator.Foreground = status.StatusTextColor;
                }, error =>
                {
                    statusIndicator.Content = Strings.StatusNotReady;
                    statusIndicator.Foreground = Brushes.Black;
                });
        }
    }
}
