using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
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

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices
{
    internal class ChannelService : MediaService
    {
        public IObservable<IEnumerable<ChannelModel>> Channels =>
            Observable.Create<IEnumerable<ChannelModel>>(subscriber =>
            {
                JObject json = GetServiceInfo(MediaServicesConstants.Paths.Channels.List);

                if (json == null)
                {
                    subscriber.OnError(new ServiceStatusException("Response is invalid"));
                    return Disposable.Empty;
                }

                IEnumerable<ChannelModel> channels = json.SelectToken(MediaServicesConstants.Json.Value).Select(channel =>
                {
                    string status = channel.SelectToken(MediaServicesConstants.Json.Status).Value<string>();

                    return new ChannelModel()
                    {
                        Id = channel.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                        Name = channel.SelectToken(MediaServicesConstants.Json.Name).Value<string>(),
                        Status = MapStatus(status)
                    };
                }).Where(channel =>
                {
                    return MediaServicesConfigurationRepository.Channels.Contains(channel.Name);
                });

                subscriber.OnNext(channels);
                subscriber.OnCompleted();
                return Disposable.Empty;
            });

        public IObservable<bool> StartAll =>
            Observable.Create<bool>(subscriber =>
            {
                bool successfullyStartedAll = true;

                MediaServicesRepository.Channels.ForEach(channel =>
                {
                    if (!successfullyStartedAll)
                    {
                        return;
                    }

                    string path = string.Format(MediaServicesConstants.Paths.Channels.Start, channel.Id);

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
                    subscriber.OnError(new StartUpException("Could not start up one or more channels"));
                    return Disposable.Empty;
                }

                subscriber.OnNext(true);
                subscriber.OnCompleted();
                return Disposable.Empty;
            });
    }
}
