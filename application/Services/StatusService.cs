using LiteralLifeChurch.LiveStreamingController.Enums;
using LiteralLifeChurch.LiveStreamingController.Models;
using Microsoft.WindowsAzure.MediaServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Services
{
    public class StatusService
    {

        private readonly AuthenticationService authService = new AuthenticationService();

        public IObservable<ServiceStatus> GetStatus()
        {

            List<IObservable<IEnumerable<IStatus>>> statusRequests = new List<IObservable<IEnumerable<IStatus>>>()
            {
                GetChannelStatus().SubscribeOn(NewThreadScheduler.Default) as IObservable<IEnumerable<IStatus>>,
                GetEndpointStatus().SubscribeOn(NewThreadScheduler.Default) as IObservable<IEnumerable<IStatus>>,
                GetProgramStatus().SubscribeOn(NewThreadScheduler.Default) as IObservable<IEnumerable<IStatus>>
            };

            return Observable.Zip(statusRequests, (statuses) =>
            {

                List<ChannelStatus> channels = (statuses[0] as IEnumerable<ChannelStatus>).ToList();
                List<EndpointStatus> endpoints = (statuses[1] as IEnumerable<EndpointStatus>).ToList();
                List<ProgramStatus> programs = (statuses[2] as IEnumerable<ProgramStatus>).ToList();

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

                return new ServiceStatus()
                {
                    Channels = channels,
                    Endpoints = endpoints,
                    Programs = programs,
                    Summary = summary
                };
            });
        }

        // region Private Methods

        private IObservable<IEnumerable<ChannelStatus>> GetChannelStatus()
        {
            return Observable.Create<IEnumerable<ChannelStatus>>(subscriber =>
            {
                CloudMediaContext context = authService.GetContext();
                ChannelBaseCollection channels = context.Channels;
                List<string> validChannels = AuthenticationService.Channels;

                IEnumerable<ChannelStatus> statuses = channels.AsEnumerable().Where(channel =>
                {
                    return validChannels.Contains(channel.Name);
                }).Select(channel =>
                {
                    ChannelStatus info = new ChannelStatus()
                    {
                        Name = channel.Name
                    };

                    switch (channel.State)
                    {
                        case ChannelState.Running:
                            info.Status = StatusType.Ready;
                            break;

                        case ChannelState.Starting:
                            info.Status = StatusType.Starting;
                            break;

                        case ChannelState.Deleting:
                        case ChannelState.Stopped:
                        case ChannelState.Stopping:
                        default:
                            info.Status = StatusType.NotReady;
                            break;
                    }

                    return info;
                });

                subscriber.OnNext(statuses);
                return Disposable.Empty;
            });
        }

        private IObservable<IEnumerable<EndpointStatus>> GetEndpointStatus()
        {
            return Observable.Create<IEnumerable<EndpointStatus>>(subscriber =>
            {
                CloudMediaContext context = authService.GetContext();
                StreamingEndpointBaseCollection endpoints = context.StreamingEndpoints;
                List<string> validEndpoints = AuthenticationService.StreamingEndpoints;

                IEnumerable<EndpointStatus> statuses = endpoints.AsEnumerable().Where(endpoint =>
                {
                    return validEndpoints.Contains(endpoint.Name);
                }).Select(endpoint =>
                {
                    EndpointStatus info = new EndpointStatus()
                    {
                        Name = endpoint.Name
                    };

                    switch (endpoint.State)
                    {
                        case StreamingEndpointState.Running:
                            info.Status = StatusType.Ready;
                            break;

                        case StreamingEndpointState.Scaling:
                        case StreamingEndpointState.Starting:
                            info.Status = StatusType.Starting;
                            break;

                        case StreamingEndpointState.Deleting:
                        case StreamingEndpointState.Stopped:
                        case StreamingEndpointState.Stopping:
                        default:
                            info.Status = StatusType.NotReady;
                            break;
                    }

                    return info;
                });

                subscriber.OnNext(statuses);
                return Disposable.Empty;
            });
        }

        private IObservable<IEnumerable<ProgramStatus>> GetProgramStatus()
        {
            return Observable.Create<IEnumerable<ProgramStatus>>(subscriber =>
            {
                CloudMediaContext context = authService.GetContext();
                ProgramBaseCollection programs = context.Programs;
                List<string> validPrograms = AuthenticationService.Programs;

                IEnumerable<ProgramStatus> statuses = programs.AsEnumerable().Where(program =>
                {
                    return validPrograms.Contains(program.Name);
                }).Select(program =>
                {
                    ProgramStatus info = new ProgramStatus()
                    {
                        Name = program.Name
                    };

                    switch (program.State)
                    {
                        case ProgramState.Running:
                            info.Status = StatusType.Ready;
                            break;

                        case ProgramState.Starting:
                            info.Status = StatusType.Starting;
                            break;

                        case ProgramState.Stopped:
                        case ProgramState.Stopping:
                        default:
                            info.Status = StatusType.NotReady;
                            break;
                    }

                    return info;
                });

                subscriber.OnNext(statuses);
                return Disposable.Empty;
            });
        }

        // endregion
    }
}
