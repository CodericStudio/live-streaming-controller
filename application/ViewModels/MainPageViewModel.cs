using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Status;
using LiteralLifeChurch.LiveStreamingController.Services.Azure;
using System;
using System.Reactive.Linq;
using Windows.UI;

namespace LiteralLifeChurch.LiveStreamingController.ViewModels
{
    internal class MainPageViewModel : IViewModel
    {
        private readonly Workflow Workflow = new Workflow();

        public IObservable<ServiceStatusDesignUpdateModel> MapToDisplayableStatus(ServiceStatusModel status)
        {
            ServiceStatusDesignUpdateModel displayableStatus;

            switch (status.Summary)
            {
                case StatusType.Ready:
                    displayableStatus = new ServiceStatusDesignUpdateModel()
                    {
                        ButtonTextResource = "StreamingStop",
                        StatusTextColor = Colors.Green,
                        StatusTextResource = "StatusReady"
                    };

                    break;

                case StatusType.Starting:
                    displayableStatus = new ServiceStatusDesignUpdateModel()
                    {
                        ButtonTextResource = "StreamingWorking",
                        StatusTextColor = Colors.Goldenrod,
                        StatusTextResource = "StatusStarting"
                    };

                    break;

                case StatusType.Stopping:
                    displayableStatus = new ServiceStatusDesignUpdateModel()
                    {
                        ButtonTextResource = "StreamingWorking",
                        StatusTextColor = Colors.Goldenrod,
                        StatusTextResource = "StatusStopping"
                    };

                    break;

                default:
                    displayableStatus = new ServiceStatusDesignUpdateModel()
                    {
                        ButtonTextResource = "StreamingStart",
                        StatusTextColor = Colors.Red,
                        StatusTextResource = "StatusNotReady"
                    };

                    break;
            };

            return Observable.Return(displayableStatus);
        }

        public IObservable<ServiceStatusModel> StartStreaming =>
            Workflow.Start;

        public IObservable<ServiceStatusModel> Status =>
            Workflow.Status;

        public IObservable<ServiceStatusModel> StopStreaming =>
            Workflow.Stop;
    }
}
