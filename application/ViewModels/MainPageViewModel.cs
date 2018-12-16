using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Status;
using LiteralLifeChurch.LiveStreamingController.Repositories.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Services.Azure;
using System;
using System.Reactive.Linq;
using Windows.UI;

namespace LiteralLifeChurch.LiveStreamingController.ViewModels
{
    internal class MainPageViewModel : IViewModel
    {
        private readonly AggregationService AggregationService = new AggregationService();

        public void CacheServices(ServiceStatusModel status)
        {
            MediaServicesRepository.Channels = status.Channels;
            MediaServicesRepository.Endpoints = status.Endpoints;
        }

        public IObservable<ServiceStatusDesignUpdateModel> MapToDisplayableStatus(ServiceStatusModel status)
        {
            ServiceStatusDesignUpdateModel displayableStatus;

            switch (status.Summary)
            {
                case StatusType.Ready:
                    displayableStatus = new ServiceStatusDesignUpdateModel()
                    {
                        StatusTextColor = Colors.Green,
                        StatusTextResource = "StatusReady"
                    };

                    break;

                case StatusType.Starting:
                    displayableStatus = new ServiceStatusDesignUpdateModel()
                    {
                        StatusTextColor = Colors.Goldenrod,
                        StatusTextResource = "StatusStarting"
                    };

                    break;

                default:
                    displayableStatus = new ServiceStatusDesignUpdateModel()
                    {
                        StatusTextColor = Colors.Red,
                        StatusTextResource = "StatusNotReady"
                    };

                    break;
            };

            return Observable.Return(displayableStatus);
        }

        public IObservable<ServiceStatusModel> StartStreaming =>
            AggregationService.Start;

        public IObservable<ServiceStatusModel> Status =>
            AggregationService.Status;
    }
}
