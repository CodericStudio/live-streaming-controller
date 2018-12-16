using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.DataCreation;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow;
using LiteralLifeChurch.LiveStreamingController.Services.Network;
using LiteralLifeChurch.LiveStreamingController.Utilities;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices
{
    internal class LocatorService : MediaService
    {
        public IObservable<LocatorStepWorkflowModel> Create(string assetId, string accessPolicyId, ProgramModel program)
        {
            return Observable.Create<LocatorStepWorkflowModel>(subscriber =>
            {
                string path = MediaServicesConstants.Paths.Locators.Create;
                RetryRestClient client = GenerateClient(path);

                CreateLocatorModel payload = new CreateLocatorModel
                {
                    AccessPolicyId = accessPolicyId,
                    AssetId = assetId,
                    Name = GenerateLocatorName(),
                    Type = MediaServicesConstants.Conventions.Locators.Type
                };

                RestRequest request = GenerateAuthenticatedRequest(Method.POST);
                request.AddJsonBody(payload);
                request.RequestFormat = DataFormat.Json;

                IRestResponse response = client.Execute(request);

                if (!HttpUtils.Is2xx(response.StatusCode))
                {
                    subscriber.OnError(new LocatorException());
                    return null;
                }

                JObject json = JObject.Parse(response.Content);

                if (json == null)
                {
                    subscriber.OnError(new LocatorException());
                    return null;
                }

                subscriber.OnNext(new LocatorStepWorkflowModel()
                {
                    Path = json.SelectToken(MediaServicesConstants.Json.Path).Value<string>(),
                    Program = program
                });

                subscriber.OnCompleted();

                return Disposable.Empty;
            });
        }

        private string GenerateLocatorName()
        {
            string guid = Guid.NewGuid().ToString();
            return $"{MediaServicesConstants.Conventions.Locators.NamePrefix}{guid}";
        }
    }
}
