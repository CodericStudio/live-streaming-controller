using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using LiteralLifeChurch.LiveStreamingController.Repositories.Azure.Authentication;
using LiteralLifeChurch.LiveStreamingController.Services.Network;
using LiteralLifeChurch.LiveStreamingController.Utilities;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices
{
    internal abstract class MediaService<ServiceType> where ServiceType : IMediaServicesModel
    {

        protected delegate string UrlExpansion(ServiceType service);

        // region RestSharp Helpers

        protected RetryRestClient GenerateClient(string path)
        {
            string cleanUrlBase = AuthenticationConfigurationRepository.RestApiEndpoint.TrimEnd('/');
            string cleanUrlPath = path.TrimStart('/');
            string fullUrl = $"{cleanUrlBase}/{cleanUrlPath}";

            return new RetryRestClient(fullUrl);
        }

        protected RestRequest GenerateAuthenticatedRequest(Method method = Method.GET)
        {
            string bearerToken = $"{MediaServicesConstants.Headers.Authorization.Item2} {TokenRepository.AccessToken}";

            RestRequest request = new RestRequest(method);
            request.AddHeader(MediaServicesConstants.Headers.AcceptHeader.Item1, MediaServicesConstants.Headers.AcceptHeader.Item2);
            request.AddHeader(MediaServicesConstants.Headers.Authorization.Item1, bearerToken);
            request.AddHeader(MediaServicesConstants.Headers.MsVersionHeader.Item1, MediaServicesConstants.Headers.MsVersionHeader.Item2);

            return request;
        }

        protected JObject GetServiceInfo(string path)
        {
            RetryRestClient client = GenerateClient(path);
            RestRequest request = GenerateAuthenticatedRequest();
            IRestResponse response = client.Execute(request);

            return JObject.Parse(response.Content);
        }

        // endregion

        // region Shared Logic

        protected IObservable<bool> DeleteAll(IObservable<IEnumerable<ServiceType>> services, UrlExpansion pathExpansion)
        {
            return services
                .SelectMany(service => service)
                .SelectMany(service =>
                {
                    string url = pathExpansion(service);

                    RetryRestClient client = GenerateClient(url);
                    RestRequest request = GenerateAuthenticatedRequest(Method.DELETE);
                    client.Execute(request);

                    return Observable.Return(true);
                })
                .ToList()
                .Select(outcomes =>
                {
                    return true;
                });
        }

        protected IObservable<bool> StartAll(IObservable<IEnumerable<ServiceType>> services, UrlExpansion pathExpansion)
        {
            return services
                .SelectMany(service => service)
                .SelectMany(service =>
                {
                    string url = pathExpansion(service);

                    RetryRestClient client = GenerateClient(url);
                    RestRequest request = GenerateAuthenticatedRequest(Method.POST);
                    IRestResponse response = client.Execute(request);

                    if (!HttpUtils.Is2xx(response.StatusCode))
                    {
                        throw new StartUpException($"Could not start service of type: {typeof(ServiceType).Name}");
                    }

                    return Observable.Return(true);
                })
                .ToList()
                .Select(outcomes =>
                {
                    return true;
                });
        }

        protected IObservable<bool> StartAll(IEnumerable<ServiceType> services, UrlExpansion pathExpansion)
        {
            return StartAll(Observable.Return(services), pathExpansion);
        }

        protected IObservable<bool> StopAll(IObservable<IEnumerable<ServiceType>> services, UrlExpansion urlExpansion)
        {
            return services
                .SelectMany(service => service)
                .SelectMany(service =>
                {
                    string url = urlExpansion(service);

                    RetryRestClient client = GenerateClient(url);
                    RestRequest request = GenerateAuthenticatedRequest(Method.POST);
                    client.Execute(request);

                    return Observable.Return(true);
                })
                .ToList()
                .Select(outcomes =>
                {
                    return true;
                });
        }

        // endregion

        internal StatusType MapStatus(string status)
        {
            status = status.ToLower().Trim();

            switch (status)
            {
                case MediaServicesConstants.Statuses.Scaling:
                case MediaServicesConstants.Statuses.Starting:
                    return StatusType.Starting;

                case MediaServicesConstants.Statuses.Stopping:
                    return StatusType.Stopping;

                case MediaServicesConstants.Statuses.Running:
                    return StatusType.Ready;

                default:
                    return StatusType.NotReady;
            }
        }
    }
}
