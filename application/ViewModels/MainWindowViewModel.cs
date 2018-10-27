using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Status;
using LiteralLifeChurch.LiveStreamingController.Resources;
using LiteralLifeChurch.LiveStreamingController.Services;
using LiteralLifeChurch.LiveStreamingController.Services.Azure;
using System;
using System.Reactive.Linq;
using System.Windows.Media;

namespace LiteralLifeChurch.LiveStreamingController.ViewModels
{
    internal class MainWindowViewModel : IViewModel
    {
        private StatusService StatusService = new StatusService();

        public IObservable<ServiceStatusDesignUpdateModel> Status =>
            StatusService.Status
                .Select(status =>
                {
                    switch (status.Summary)
                    {
                        case StatusType.Ready:
                            return new ServiceStatusDesignUpdateModel()
                            {
                                StatusText = Strings.StatusReady,
                                StatusTextColor = Brushes.Green
                            };

                        case StatusType.Starting:
                            return new ServiceStatusDesignUpdateModel()
                            {
                                StatusText = Strings.StatusStarting,
                                StatusTextColor = Brushes.Goldenrod
                            };

                        default:
                            return new ServiceStatusDesignUpdateModel()
                            {
                                StatusText = Strings.StatusNotReady,
                                StatusTextColor = Brushes.Red
                            };
                    };
                });
    }
}
