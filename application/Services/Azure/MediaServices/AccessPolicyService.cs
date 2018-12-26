using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.DataCreation;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.Workflow;
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
    internal class AccessPolicyService : MediaService<AccessPolicyModel>
    {
        public IObservable<AccessPolicyStepWorkflowModel> Create(string assetId, ChannelModel channel, ProgramModel program)
        {
            return Observable.Create<AccessPolicyStepWorkflowModel>(subscriber =>
            {
                string path = MediaServicesConstants.Paths.AccessPolicies.Create;
                RetryRestClient client = GenerateClient(path);

                CreateAccessPolicyModel payload = new CreateAccessPolicyModel
                {
                    DurationInMinutes = MediaServicesConstants.Conventions.AccessPolicies.DurationInMinutes.ToString(),
                    Name = GenerateAccessPolicyName(),
                    Permissions = MediaServicesConstants.Conventions.AccessPolicies.Permissions
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
                    Channel = channel,
                    Program = program
                });

                subscriber.OnCompleted();

                return Disposable.Empty;
            });
        }

        public new IObservable<bool> DeleteAll => DeleteAll(AccessPolicies, accessPolicy =>
            string.Format(MediaServicesConstants.Paths.AccessPolicies.Delete, accessPolicy.Id)
        );

        private IObservable<IEnumerable<AccessPolicyModel>> AccessPolicies =>
            Observable.Create<IEnumerable<AccessPolicyModel>>(subscriber =>
            {
                JObject json = GetServiceInfo(MediaServicesConstants.Paths.AccessPolicies.List);

                if (json == null)
                {
                    subscriber.OnError(new ServiceStatusException("Response is invalid"));
                    return Disposable.Empty;
                }

                IEnumerable<AccessPolicyModel> accessPolicies = json.SelectToken(MediaServicesConstants.Json.Value).Select(accessPolicy =>
                {
                    return new AccessPolicyModel
                    {
                        DurationInMinutes = accessPolicy.SelectToken(MediaServicesConstants.Json.DurationInMinutes).Value<int>(),
                        Id = accessPolicy.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                        Name = accessPolicy.SelectToken(MediaServicesConstants.Json.Name).Value<string>(),
                        Permissions = accessPolicy.SelectToken(MediaServicesConstants.Json.Permissions).Value<int>()
                    };
                }).Where(accessPolicy =>
                {
                    return Regex.IsMatch(accessPolicy.Name, MediaServicesConstants.Conventions.AccessPolicies.RegexSelector);
                });

                subscriber.OnNext(accessPolicies);
                subscriber.OnCompleted();
                return Disposable.Empty;
            });

        private string GenerateAccessPolicyName()
        {
            string guid = Guid.NewGuid().ToString();
            return $"{MediaServicesConstants.Conventions.AccessPolicies.NamePrefix}{guid}";
        }
    }
}
