using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.DataCreation;
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
    internal class AssetService : MediaService
    {
        public IObservable<AssetStepWorkflowModel> Create(string channelId)
        {
            return Observable.Create<AssetStepWorkflowModel>(subscriber =>
            {
                string path = MediaServicesConstants.Paths.Assets.Create;
                RetryRestClient client = GenerateClient(path);

                CreateAssetModel payload = new CreateAssetModel
                {
                    Name = GenerateAssetName()
                };

                RestRequest request = GenerateAuthenticatedRequest(Method.POST);
                request.AddJsonBody(payload);
                request.RequestFormat = DataFormat.Json;

                IRestResponse response = client.Execute(request);

                if (!HttpUtils.Is2xx(response.StatusCode))
                {
                    subscriber.OnError(new AssetException());
                    return null;
                }

                JObject json = JObject.Parse(response.Content);

                if (json == null)
                {
                    subscriber.OnError(new AssetException());
                    return null;
                }

                subscriber.OnNext(new AssetStepWorkflowModel()
                {
                    AssetId = json.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                    ChannelId = channelId
                });

                subscriber.OnCompleted();
                return Disposable.Empty;
            });
        }

        private string GenerateAssetName()
        {
            string guid = Guid.NewGuid().ToString();
            return $"{MediaServicesConstants.Conventions.Assets.NamePrefix}{guid}";
        }
    }
}
