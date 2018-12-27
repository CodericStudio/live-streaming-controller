using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Status;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow;
using LiteralLifeChurch.LiveStreamingController.Models.Firebase.Workflow;
using LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Services.Firebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure
{
    internal class Workflow
    {
        private readonly AccessPolicyService accessPolicyService = new AccessPolicyService();
        private readonly AssetFileService assetFileService = new AssetFileService();
        private readonly AssetService assetService = new AssetService();
        private readonly ChannelService channelService = new ChannelService();
        private readonly StreamingEndpointService endpointService = new StreamingEndpointService();
        private readonly FirebaseService firebaseService = new FirebaseService();
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
         *     Emit: Service Status model
         *     
         * [3] Create one asset for each program for [4]
         *     Emit: Asset ID and Channel model
         * 
         * [4] Create one program under each channel from [1] and associate it with created asset from [3]
         *     Emit: Asset ID, Channel model, and Program model
         *     
         * [5] Obtain the name of the .ism file created for the program in [4] inside of the asset created in [3]
         *     Emit: Asset ID, Asset File model, Channel model, and program model
         * 
         * [6] Create a read-only access policy for [6]
         *     Emit: Access policy ID, Asset ID, Asset File model, Channel model, and Program model
         * 
         * [7] Create a locator and associate it with policy from [5] and asset from [3]
         *     Emit: Asset File model, Locator path, Channel model, and Program model
         *     
         * [8] Send the locator URL created in [6] for each program created in [4] to Firebase
         *     Emit: Program model
         * 
         * [9] Start all programs created in [4]
         *     Emit: Boolean indicating whether the programs stated
         * 
         * [10] Wait for all programs from [4] to start
         *      Emit: Service Status model
         */

        public IObservable<ServiceStatusModel> Start {
            get {

                List<IObservable<bool>> startUpServices = new List<IObservable<bool>>()
                {
                    channelService.StartAll.SubscribeOn(NewThreadScheduler.Default),
                    endpointService.StartAll.SubscribeOn(NewThreadScheduler.Default)
                };

                return Observable.Zip(startUpServices, startedServicesPayload => // [1]
                {
                    return true;
                }).SelectMany(startedServicesPayload => // [2]
                {
                    return HammerPollUntil(
                        allChannels: StatusType.Ready,
                        allEndpoints: StatusType.Ready,
                        allPrograms: null // Don't care, creating these later
                    );
                }).SelectMany(serviceStatusesPayload => // [3]
                {
                    IEnumerable<IObservable<AssetStepWorkflowModel>> createAssets = serviceStatusesPayload.Channels.Select(channelPayload =>
                    {
                        return assetService.Create(channelPayload).SubscribeOn(NewThreadScheduler.Default);
                    });

                    return Observable.Zip(createAssets);
                }).SelectMany(createdAssetsPayload => // [4]
                {
                    IEnumerable<IObservable<ProgramStepWorkflowModel>> createPrograms = createdAssetsPayload.Select(assetPayload =>
                    {
                        return programService.Create(assetPayload.Channel, assetPayload.AssetId).SubscribeOn(NewThreadScheduler.Default);
                    });

                    return Observable.Zip(createPrograms);
                }).SelectMany(createdProgramsPayload => // [5]
                {
                    IEnumerable<IObservable<AssetFileStepWorkflowModel>> obtainAssetFiles = createdProgramsPayload.Select(programPayload =>
                    {
                        return assetFileService.GetIsmFile(programPayload.AssetId, programPayload.Channel, programPayload.Program).SubscribeOn(NewThreadScheduler.Default);
                    });

                    return Observable.Zip(obtainAssetFiles);
                }).SelectMany(assetFilesPayload => // [6]
                {
                    IEnumerable<IObservable<AccessPolicyStepWorkflowModel>> createAccessPolicies = assetFilesPayload.Select(assetFilePayload =>
                    {
                        return accessPolicyService.Create(assetFilePayload.AssetId, assetFilePayload.AssetFile, assetFilePayload.Channel, assetFilePayload.Program).SubscribeOn(NewThreadScheduler.Default);
                    });

                    return Observable.Zip(createAccessPolicies);
                }).SelectMany(createdAccessPoliciesPayload => // [7]
                {
                    IEnumerable<IObservable<LocatorStepWorkflowModel>> createLocators = createdAccessPoliciesPayload.Select(accessPolicyPayload =>
                    {
                        return locatorService.Create(accessPolicyPayload.AssetId, accessPolicyPayload.AccessPolicyId, accessPolicyPayload.AssetFile, accessPolicyPayload.Channel, accessPolicyPayload.Program).SubscribeOn(NewThreadScheduler.Default);
                    });

                    return Observable.Zip(createLocators);
                }).SelectMany(createdLocatorsPayload => // [8]
                {
                    IEnumerable<IObservable<FirebaseStepWorkflowModel>> firebaseUpdates = createdLocatorsPayload.Select(locatorPayload =>
                    {
                        return firebaseService.PublishUrl(locatorPayload.Channel.Name, locatorPayload.Path, locatorPayload.AssetFile.Name, locatorPayload.Program);
                    });

                    return Observable.Zip(firebaseUpdates);
                }).SelectMany(createdLocatorsPayload => // [9]
                {
                    List<ProgramModel> programsToStart = createdLocatorsPayload.Select(locatorPayload =>
                    {
                        return locatorPayload.Program;
                    }).ToList();

                    return programService.StartAll(programsToStart);
                }).SelectMany(startedProgramsPayload => // [10]
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

                    bool anyStopping =
                        channels.Any(x => x.Status == StatusType.Stopping) ||
                        endpoints.Any(x => x.Status == StatusType.Stopping) ||
                        programs.Any(x => x.Status == StatusType.Stopping);

                    bool anyNotReady =
                        channels.Any(x => x.Status == StatusType.NotReady) ||
                        endpoints.Any(x => x.Status == StatusType.NotReady) ||
                        programs.Any(x => x.Status == StatusType.NotReady);

                    StatusType summary = StatusType.Ready;

                    if (anyStarting)
                    {
                        summary = StatusType.Starting;
                    }
                    else if (anyStopping)
                    {
                        summary = StatusType.Stopping;
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

        public IObservable<ServiceStatusModel> Stop => programService
            .StopAll
            .SelectMany(stoppedProgramsPayload => endpointService.StopAll)
            .SelectMany(stoppedEndpointsPayload =>
                 HammerPollUntil(
                    allChannels: null, // Don't care, stopping these later
                    allEndpoints: StatusType.NotReady,
                    allPrograms: StatusType.NotReady
                )
            )
            .SelectMany(serviceStatusesPayload => channelService.StopAll)
            .SelectMany(stoppedChannelsPayload =>
                HammerPollUntil(
                    allChannels: StatusType.NotReady,
                    allEndpoints: null, // Already stopped
                    allPrograms: null   // these earlier
                )
            )
            .SelectMany(serviceStatusesPayload => programService.DeleteAll)
            .SelectMany(deletedProgramsPayload => locatorService.DeleteAll)
            .SelectMany(deletedLocatorsPayload => accessPolicyService.DeleteAll)
            .SelectMany(deletedAccessPoliciesPayload => assetService.DeleteAll)
            .SelectMany(deletedAssetsPayload => firebaseService.DeleteAll)
            .SelectMany(deletedCollectionPayload => Status);

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
