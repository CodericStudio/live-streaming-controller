using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.DataCreation;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow;
using LiteralLifeChurch.LiveStreamingController.Repositories.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Services.Network;
using LiteralLifeChurch.LiveStreamingController.Utilities;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices
{
    internal class ProgramService : MediaService
    {
        public IObservable<ProgramStepWorkflowModel> Create(string channelId, string assetId)
        {
            return Observable.Create<ProgramStepWorkflowModel>(subscriber =>
            {
                string path = MediaServicesConstants.Paths.Programs.Create;
                RetryRestClient client = GenerateClient(path);

                CreateProgramModel payload = new CreateProgramModel
                {
                    ArchiveWindowLength = MediaServicesConfigurationRepository.ProgramArchiveWindowDuration,
                    AssetId = assetId,
                    ChannelId = channelId,
                    Description = string.Empty,
                    Name = GenerateAssetName()
                };

                RestRequest request = GenerateAuthenticatedRequest(Method.POST);
                request.AddJsonBody(payload);
                request.RequestFormat = DataFormat.Json;

                IRestResponse response = client.Execute(request);

                if (!HttpUtils.Is2xx(response.StatusCode))
                {
                    subscriber.OnError(new ProgramException());
                    return null;
                }

                JObject json = JObject.Parse(response.Content);

                if (json == null)
                {
                    subscriber.OnError(new ProgramException());
                    return null;
                }

                subscriber.OnNext(new ProgramStepWorkflowModel()
                {
                    AssetId = assetId,
                    Program = new ProgramModel()
                    {
                        Id = json.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                        Name = json.SelectToken(MediaServicesConstants.Json.Name).Value<string>(),
                        Status = StatusType.NotReady
                    }
                });

                subscriber.OnCompleted();
                return Disposable.Empty;
            });
        }

        public IObservable<IEnumerable<ProgramModel>> Programs =>
            Observable.Create<IEnumerable<ProgramModel>>(subscriber =>
            {
                JObject json = GetServiceInfo(MediaServicesConstants.Paths.Programs.List);

                if (json == null)
                {
                    subscriber.OnError(new ServiceStatusException("Response is invalid"));
                    return Disposable.Empty;
                }

                IEnumerable<ProgramModel> programs = json.SelectToken(MediaServicesConstants.Json.Value).Select(program =>
                {
                    string status = program.SelectToken(MediaServicesConstants.Json.Status).Value<string>();

                    return new ProgramModel()
                    {
                        Id = program.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                        Name = program.SelectToken(MediaServicesConstants.Json.Name).Value<string>(),
                        Status = MapStatus(status)
                    };
                }).Where(program =>
                {
                    return Regex.IsMatch(program.Name, MediaServicesConstants.Conventions.Programs.RegexSelector);
                });

                subscriber.OnNext(programs);
                subscriber.OnCompleted();
                return Disposable.Empty;
            });

        public IObservable<bool> StartAll(List<ProgramModel> programs)
        {
            return Observable.Create<bool>(subscriber =>
            {
                bool successfullyStartedAll = true;

                programs.ForEach(program =>
                {
                    if (!successfullyStartedAll)
                    {
                        return;
                    }

                    string path = string.Format(MediaServicesConstants.Paths.Programs.Start, program.Id);

                    RetryRestClient client = GenerateClient(path);
                    RestRequest request = GenerateAuthenticatedRequest(Method.POST);
                    IRestResponse response = client.Execute(request);

                    if (!HttpUtils.Is2xx(response.StatusCode))
                    {
                        successfullyStartedAll = false;
                    }
                });

                if (!successfullyStartedAll)
                {
                    subscriber.OnError(new StartUpException("Could not start up one or more programs"));
                    return Disposable.Empty;
                }

                subscriber.OnNext(true);
                subscriber.OnCompleted();
                return Disposable.Empty;
            });
        }

        private string GenerateAssetName()
        {
            string guid = Guid.NewGuid().ToString();
            return $"{MediaServicesConstants.Conventions.Programs.NamePrefix}{guid}";
        }
    }
}
