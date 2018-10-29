using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Status;
using LiteralLifeChurch.LiveStreamingController.Services.Azure;
using System;
using System.Reactive.Linq;
using Windows.UI;

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
                                StatusTextColor = Colors.Green,
                                StatusTextResource = "StatusReady"
                            };

                        case StatusType.Starting:
                            return new ServiceStatusDesignUpdateModel()
                            {
                                StatusTextColor = Colors.Goldenrod,
                                StatusTextResource = "StatusStarting"
                            };

                        default:
                            return new ServiceStatusDesignUpdateModel()
                            {
                                StatusTextColor = Colors.Red,
                                StatusTextResource = "StatusNotReady"
                            };
                    };
                });
    }
}
