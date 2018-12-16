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
    internal class AccessPolicyService : MediaService
    {
        public IObservable<AccessPolicyStepWorkflowModel> Create(string assetId, ProgramModel program)
        {
            return Observable.Create<AccessPolicyStepWorkflowModel>(subscriber =>
            {
                string path = MediaServicesConstants.Paths.AccessPolicy.Create;
                RetryRestClient client = GenerateClient(path);

                CreateAccessPolicyModel payload = new CreateAccessPolicyModel
                {
                    DurationInMinutes = MediaServicesConstants.Conventions.AccessPolicy.DurationInMinutes.ToString(),
                    Name = GenerateAccessPolicyName(),
                    Permissions = MediaServicesConstants.Conventions.AccessPolicy.Permissions
                };

                RestRequest request = GenerateAuthenticatedRequest(Method.POST);
                request.AddJsonBody(payload);
                request.RequestFormat = DataFormat.Json;

                IRestResponse response = client.Execute(request);

                if (!HttpUtils.Is2xx(response.StatusCode))
                {
                    subscriber.OnError(new AccessPolicyException());
                    return null;
                }

                JObject json = JObject.Parse(response.Content);

                if (json == null)
                {
                    subscriber.OnError(new AccessPolicyException());
                    return null;
                }

                subscriber.OnNext(new AccessPolicyStepWorkflowModel()
                {
                    AccessPolicyId = json.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                    AssetId = assetId,
                    Program = program
                });

                subscriber.OnCompleted();

                return Disposable.Empty;
            });
        }

        private string GenerateAccessPolicyName()
        {
            string guid = Guid.NewGuid().ToString();
            return $"{MediaServicesConstants.Conventions.AccessPolicy.NamePrefix}{guid}";
        }
    }
}
