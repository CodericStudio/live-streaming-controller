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
    internal class AssetService : MediaService<AssetModel>
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

        public new IObservable<bool> DeleteAll => DeleteAll(Assets, asset =>
            string.Format(MediaServicesConstants.Paths.Assets.Delete, asset.Id)
        );

        private IObservable<IEnumerable<AssetModel>> Assets =>
            Observable.Create<IEnumerable<AssetModel>>(subscriber =>
            {
                JObject json = GetServiceInfo(MediaServicesConstants.Paths.Assets.List);

                if (json == null)
                {
                    subscriber.OnError(new ServiceStatusException("Response is invalid"));
                    return Disposable.Empty;
                }

                IEnumerable<AssetModel> assets = json.SelectToken(MediaServicesConstants.Json.Value).Select(asset =>
                {
                    return new AssetModel
                    {
                        Id = asset.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                        Name = asset.SelectToken(MediaServicesConstants.Json.Name).Value<string>()
                    };
                }).Where(asset =>
                {
                    return Regex.IsMatch(asset.Name, MediaServicesConstants.Conventions.Assets.RegexSelector);
                });

                subscriber.OnNext(assets);
                subscriber.OnCompleted();
                return Disposable.Empty;
            });

        private string GenerateAssetName()
        {
            string guid = Guid.NewGuid().ToString();
            return $"{MediaServicesConstants.Conventions.Assets.NamePrefix}{guid}";
        }
    }
}
