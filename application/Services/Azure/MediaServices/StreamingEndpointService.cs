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
    internal class StreamingEndpointService : MediaService
    {
        public IObservable<IEnumerable<EndpointModel>> Endpoints =>
            Observable.Create<IEnumerable<EndpointModel>>(subscriber =>
            {
                JObject json = GetServiceInfo(MediaServicesConstants.Paths.StreamingEndpoints.List);

                if (json == null)
                {
                    subscriber.OnError(new ServiceStatusException("Response is invalid"));
                    return Disposable.Empty;
                }

                IEnumerable<EndpointModel> endpoints = json.SelectToken(MediaServicesConstants.Json.Value).Select(endpoint =>
                {
                    string status = endpoint.SelectToken(MediaServicesConstants.Json.Status).Value<string>();

                    return new EndpointModel()
                    {
                        Id = endpoint.SelectToken(MediaServicesConstants.Json.Id).Value<string>(),
                        Name = endpoint.SelectToken(MediaServicesConstants.Json.Name).Value<string>(),
                        Status = MapStatus(status)
                    };
                });

                subscriber.OnNext(endpoints);
                return Disposable.Empty;
            });
    }
}
