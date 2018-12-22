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
    internal class StreamingEndpointService : MediaService<StreamingEndpointModel>
    {
        public IObservable<IEnumerable<StreamingEndpointModel>> Endpoints =>
            Observable.Create<IEnumerable<StreamingEndpointModel>>(subscriber =>
            {
                JObject json = GetServiceInfo(MediaServicesConstants.Paths.StreamingEndpoints.List);

                if (json == null)
                {
                    subscriber.OnError(new ServiceStatusException("Response is invalid"));
                    return Disposable.Empty;
                }

                IEnumerable<StreamingEndpointModel> endpoints = json.SelectToken(MediaServicesConstants.Json.Value).Select(endpoint =>
                {
                    string status = endpoint.SelectToken(MediaServicesConstants.Json.Status).Value<string>();

                    return new StreamingEndpointModel()
                    {
                        Id = endpoint.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                        Name = endpoint.SelectToken(MediaServicesConstants.Json.Name).Value<string>(),
                        Status = MapStatus(status)
                    };
                }).Where(endpoint =>
                {
                    return MediaServicesConfigurationRepository.StreamingEndpoints.Contains(endpoint.Name);
                });

                subscriber.OnNext(endpoints);
                subscriber.OnCompleted();
                return Disposable.Empty;
            });

        public new IObservable<bool> StartAll => StartAll(Endpoints, endpoint =>
            string.Format(MediaServicesConstants.Paths.StreamingEndpoints.Start, endpoint.Id)
        );

        public new IObservable<bool> StopAll => StopAll(Endpoints, endpoint =>
            string.Format(MediaServicesConstants.Paths.StreamingEndpoints.Stop, endpoint.Id)
        );
    }
}
