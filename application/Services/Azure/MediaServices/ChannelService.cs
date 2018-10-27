using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using Newtonsoft.Json.Linq;
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
                });

                subscriber.OnNext(channels);
                return Disposable.Empty;
            });
    }
}
