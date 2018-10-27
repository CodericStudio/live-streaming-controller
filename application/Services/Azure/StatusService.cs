using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Status;
using LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure
{
    internal class StatusService
    {
        private readonly ChannelService channelService = new ChannelService();
        private readonly StreamingEndpointService endpointService = new StreamingEndpointService();
        private readonly ProgramService programService = new ProgramService();

        public IObservable<ServiceStatusModel> Status {
            get {
                List<IObservable<IEnumerable<IMediaServicesModel>>> statusRequests = new List<IObservable<IEnumerable<IMediaServicesModel>>>()
                {
                    channelService.Channels.SubscribeOn(NewThreadScheduler.Default) as IObservable<IEnumerable<IMediaServicesModel>>,
                    endpointService.Endpoints.SubscribeOn(NewThreadScheduler.Default) as IObservable<IEnumerable<IMediaServicesModel>>,
                    programService.Programs.SubscribeOn(NewThreadScheduler.Default) as IObservable<IEnumerable<IMediaServicesModel>>
                };

                return Observable.Zip(statusRequests, (statuses) =>
                {

                    List<ChannelModel> channels = (statuses[0] as IEnumerable<ChannelModel>).ToList();
                    List<EndpointModel> endpoints = (statuses[1] as IEnumerable<EndpointModel>).ToList();
                    List<ProgramModel> programs = (statuses[2] as IEnumerable<ProgramModel>).ToList();

                    bool anyStarting =
                        channels.Any(x => x.Status == StatusType.Starting) ||
                        endpoints.Any(x => x.Status == StatusType.Starting) ||
                        programs.Any(x => x.Status == StatusType.Starting);

                    bool anyNotReady =
                        channels.Any(x => x.Status == StatusType.NotReady) ||
                        endpoints.Any(x => x.Status == StatusType.NotReady) ||
                        programs.Any(x => x.Status == StatusType.NotReady);

                    StatusType summary = StatusType.Ready;

                    if (anyStarting)
                    {
                        summary = StatusType.Starting;
                    }
                    else if (anyNotReady)
                    {
                        summary = StatusType.NotReady;
                    }

                    return new ServiceStatusModel()
                    {
                        Channels = channels,
                        Endpoints = endpoints,
                        Programs = programs,
                        Summary = summary
                    };
                });
            }
        }
    }
}
