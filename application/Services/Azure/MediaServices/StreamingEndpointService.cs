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
    internal class StreamingEndpointService : MediaService
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

        public IObservable<bool> StartAll => Observable.Create<bool>(subscriber =>
        {
            bool successfullyStartedAll = true;

            MediaServicesRepository.Endpoints.ForEach(endpoint =>
            {
                if (!successfullyStartedAll)
                {
                    return;
                }

                string path = $"StreamingEndpoints('{endpoint.Id}')/Start";

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
                subscriber.OnError(new StartUpException("Could not start up one or more streaming endpoints"));
                return Disposable.Empty;
            }

            subscriber.OnNext(true);
            subscriber.OnCompleted();
            return Disposable.Empty;
        });
    }
}
