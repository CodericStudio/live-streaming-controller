using LiteralLifeChurch.LiveStreamingController.Exceptions;
using LiteralLifeChurch.LiveStreamingController.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace LiteralLifeChurch.LiveStreamingController.Services
{
    public class ApiService : IService
    {
        private readonly SettingsService settingsService;

        public ApiService()
        {
            settingsService = new SettingsService();
        }

        public IObservable<StatusModel> GetStatusUntilStable()
        {
            return Observable.Create<StatusModel>(subscriber =>
            {
                bool isStable = false;

                RestClient client = new RestClient(BuildBaseUrl());
                RestRequest request = new RestRequest("status");
                AddQueryParamsToRequest(request);

                while (!isStable)
                {
                    IRestResponse response = client.Get(request);

                    if (!IsSuccessful(response))
                    {
                        subscriber.OnError(new ServerResponseException());
                        return Disposable.Empty;
                    }

                    StatusModel model = JsonConvert.DeserializeObject<StatusModel>(response.Content);
                    subscriber.OnNext(model);
                    isStable = model.Summary.Type == Enums.ResourceStatusTypeEnum.Stable;

                    if (!isStable)
                    {
                        Thread.Sleep(5000);
                    }
                }
               
                subscriber.OnCompleted();
                return Disposable.Empty;
            });
        }

        public IObservable<StatusModel> Start()
        {
            return Observable.Create<StatusModel>(subscriber =>
            {
                RestClient client = new RestClient(BuildBaseUrl());
                RestRequest request = new RestRequest("start");
                AddQueryParamsToRequest(request);

                IRestResponse response = client.Post(request);

                if (!IsSuccessful(response))
                {
                    subscriber.OnError(new ServerResponseException());
                    return Disposable.Empty;
                }

                StatusChangeModel model = JsonConvert.DeserializeObject<StatusChangeModel>(response.Content);
                subscriber.OnNext(model.Status);
                subscriber.OnCompleted();

                return Disposable.Empty;
            });
        }

        public IObservable<StatusModel> Stop()
        {
            return Observable.Create<StatusModel>(subscriber =>
            {
                RestClient client = new RestClient(BuildBaseUrl());
                RestRequest request = new RestRequest("stop");
                AddQueryParamsToRequest(request);

                IRestResponse response = client.Delete(request);

                if (!IsSuccessful(response))
                {
                    subscriber.OnError(new ServerResponseException());
                    return Disposable.Empty;
                }

                StatusChangeModel model = JsonConvert.DeserializeObject<StatusChangeModel>(response.Content);
                subscriber.OnNext(model.Status);
                subscriber.OnCompleted();

                return Disposable.Empty;
            });
        }

        // region Helper Methods

        private void AddQueryParamsToRequest(RestRequest request)
        {
            SettingsModel settings = settingsService.Storage;

            request.AddQueryParameter("code", settings.ApiKey);
            request.AddQueryParameter("endpoint", settings.StreamingEndpointName);
            request.AddQueryParameter("events", settings.LiveEventNames);
        }

        private Uri BuildBaseUrl()
        {
            SettingsModel settings = settingsService.Storage;

            UriBuilder builder = new UriBuilder
            {
                Scheme = "https",
                Host = settings.Host,
                Path = $"api/v1/broadcaster"
            };

            return builder.Uri;
        }

        private bool IsSuccessful(IRestResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.Content))
            {
                return false;
            }

            int code = Convert.ToInt32(response.StatusCode);
            return 200 <= code && code <= 299;
        }

        // endregion
    }
}
