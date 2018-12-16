using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Status;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow;
using LiteralLifeChurch.LiveStreamingController.Repositories.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure
{
    internal class AggregationService
    {
        private readonly AccessPolicyService accessPolicyService = new AccessPolicyService();
        private readonly AssetService assetService = new AssetService();
        private readonly ChannelService channelService = new ChannelService();
        private readonly StreamingEndpointService endpointService = new StreamingEndpointService();
        private readonly LocatorService locatorService = new LocatorService();
        private readonly ProgramService programService = new ProgramService();

        /**
         * To start up the workflow, these components must be in place:
         *   - Start Streaming Enpoint
         *   - Start Channel
         *   - Create Program
         *   - Create Asset
         *   - Create Locator
         *   - Create a Read-Only Access Policy
         *   
         * Therefore, these are the steps to start up the streaming services:
         * 
         * [1] Trigger all endpoints and channels to start
         *     Emit: Nothing special
         *     
         * [2] Wait for all endpoints and channels to start from [1]
         *     Emit: Service status model
         *     
         * [3] Create one asset for each program for [4]
         *     Emit: Asset ID and channel ID
         * 
         * [4] Create one program under each channel from [1] and associate it with created asset from [3]
         *     Emit: Asset ID and Program model
         * 
         * [5] Create a read-only access policy for [6]
         *     Emit: Access policy ID, Asset ID, and Program model
         * 
         * [6] Create a locator and associate it with policy from [5] and asset from [3]
         *     Emit: Locator path and Program model
         * 
         * [7] Start all programs created in [4]
         *     Emit: Boolean indicating whether the programs stated
         * 
         * [8] Wait for all programs from [4] to start
         *     Emit: Service Status model
         * 
         * [9] Send program IDs to Firebase
         */

        public IObservable<ServiceStatusModel> Start {
            get {

                List<IObservable<bool>> startUpServices = new List<IObservable<bool>>()
                {
                    channelService.StartAll.SubscribeOn(NewThreadScheduler.Default),
                    endpointService.StartAll.SubscribeOn(NewThreadScheduler.Default)
                };

                return Observable.Zip(startUpServices, startedServicesOutcome => // [1]
                {
                    return true;
                }).SelectMany(startedServicesOutcome => // [2]
                {
                    return HammerPollUntil(
                        allChannels: StatusType.Ready,
                        allEndpoints: StatusType.Ready,
                        allPrograms: null // Don't care, creating these later
                    );
                }).SelectMany(serviceStatusesPayload => // [3]
                {
                    List<IObservable<AssetStepWorkflowModel>> createAssets = MediaServicesRepository.Channels.Select(channelPayload =>
                    {
                        return assetService.Create(channelPayload.Id).SubscribeOn(NewThreadScheduler.Default);
                    }).ToList();

                    return Observable.Zip(createAssets);
                }).SelectMany(createdAssetsPayload => // [4]
                {
                    List<IObservable<ProgramStepWorkflowModel>> createPrograms = createdAssetsPayload.Select(assetPayload =>
                    {
                        return programService.Create(assetPayload.ChannelId, assetPayload.AssetId).SubscribeOn(NewThreadScheduler.Default);
                    }).ToList();

                    return Observable.Zip(createPrograms);
                }).SelectMany(createdProgramsPayload => // [5]
                {
                    List<IObservable<AccessPolicyStepWorkflowModel>> createAccessPolicies = createdProgramsPayload.Select(programPayload =>
                    {
                        return accessPolicyService.Create(programPayload.AssetId, programPayload.Program).SubscribeOn(NewThreadScheduler.Default);
                    }).ToList();

                    return Observable.Zip(createAccessPolicies);
                }).SelectMany(createdAccessPoliciesPayload => // [6]
                {
                    List<IObservable<LocatorStepWorkflowModel>> createLocators = createdAccessPoliciesPayload.Select(accessPolicyPayload =>
                    {
                        return locatorService.Create(accessPolicyPayload.AssetId, accessPolicyPayload.AccessPolicyId, accessPolicyPayload.Program).SubscribeOn(NewThreadScheduler.Default);
                    }).ToList();

                    return Observable.Zip(createLocators);
                }).SelectMany(createdLocatorsPayload => // [7]
                {
                    List<ProgramModel> programsToStart = createdLocatorsPayload.Select(locatorPayload =>
                    {
                        return locatorPayload.Program;
                    }).ToList();

                    return programService.StartAll(programsToStart);
                }).SelectMany(startedProgramsPayload => // [8]
                {
                    return HammerPollUntil(
                        allChannels: null,  // Already started
                        allEndpoints: null, // these earlier
                        allPrograms: StatusType.Ready
                    );
                });
            }
        }

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
                    List<StreamingEndpointModel> endpoints = (statuses[1] as IEnumerable<StreamingEndpointModel>).ToList();
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

        private IObservable<ServiceStatusModel> HammerPollUntil(StatusType? allChannels, StatusType? allEndpoints, StatusType? allPrograms)
        {
            return Status.SelectMany(status =>
            {
                if (StatusesMatch(status, allChannels, allEndpoints, allPrograms))
                {
                    return Observable.Return(status);
                }

                return Observable.Throw<ServiceStatusModel>(new StatusesDoNotMatchException());
            }).RetryWhen(error =>
            {
                return error.Delay(TimeSpan.FromSeconds(5));
            });
        }

        private bool StatusesMatch(ServiceStatusModel allStatuses, StatusType? allChannels, StatusType? allEndpoints, StatusType? allPrograms)
        {
            bool channelsMatch = allChannels == null || allStatuses.Channels.All(channel =>
            {
                return channel.Status == allChannels;
            });

            bool endpointsMatch = allEndpoints == null || allStatuses.Endpoints.All(endpoint =>
            {
                return endpoint.Status == allEndpoints;
            });

            bool programsMatch = allPrograms == null || allStatuses.Programs.All(program =>
            {
                return program.Status == allPrograms;
            });

            return channelsMatch && endpointsMatch && programsMatch;
        }
    }
}
