using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Repositories.Azure.MediaServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices
{
    internal class ChannelService : MediaService<ChannelModel>
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

        public new IObservable<bool> StartAll => StartAll(Channels, channel =>
            string.Format(MediaServicesConstants.Paths.Channels.Start, channel.Id)
        );

        public new IObservable<bool> StopAll => StopAll(Channels, channel =>
            string.Format(MediaServicesConstants.Paths.Channels.Stop, channel.Id)
        );
    }
}
