using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Repositories.Azure.Authentication;
using LiteralLifeChurch.LiveStreamingController.Services.Network;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.MediaServices
{
    public abstract class MediaService
    {
        internal JObject GetServiceInfo(string path)
        {
            RetryRestClient client = GenerateClient(path);
            RestRequest request = GenerateAuthenticatedRequest();
            IRestResponse response = client.Execute(request);

            return JObject.Parse(response.Content);
        }

        internal StatusType MapStatus(string status)
        {
            status = status.ToLower().Trim();

            switch (status)
            {
                case MediaServicesConstants.Statuses.Scaling:
                case MediaServicesConstants.Statuses.Starting:
                    return StatusType.Starting;

                case MediaServicesConstants.Statuses.Running:
                    return StatusType.Ready;

                default:
                    return StatusType.NotReady;
            }
        }

        // region Private Methods

        private RetryRestClient GenerateClient(string path)
        {
            string cleanUrlBase = AuthenticationConfigurationRepository.RestApiEndpoint.TrimEnd('/');
            string cleanUrlPath = path.TrimStart('/');
            string fullUrl = $"{cleanUrlBase}/{cleanUrlPath}";

            return new RetryRestClient(fullUrl);
        }

        private RestRequest GenerateAuthenticatedRequest(Method method = Method.GET)
        {
            string bearerToken = $"{MediaServicesConstants.Headers.Authorization.Item2} {TokenRepository.AccessToken}";

            RestRequest request = new RestRequest(method);
            request.AddHeader(MediaServicesConstants.Headers.AcceptHeader.Item1, MediaServicesConstants.Headers.AcceptHeader.Item2);
            request.AddHeader(MediaServicesConstants.Headers.Authorization.Item1, bearerToken);
            request.AddHeader(MediaServicesConstants.Headers.MsVersionHeader.Item1, MediaServicesConstants.Headers.MsVersionHeader.Item2);

            return request;
        }

        // endregion
    }
}
