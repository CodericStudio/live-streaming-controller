using LiteralLifeChurch.LiveStreamingController.Repositories.Azure.Authentication;
using LiteralLifeChurch.LiveStreamingController.Services.Azure;
using LiteralLifeChurch.LiveStreamingController.Services.Azure.Authentication;
using LiteralLifeChurch.LiveStreamingController.Utilities;
using RestSharp;

namespace LiteralLifeChurch.LiveStreamingController.Services.Network
{
    internal class RetryRestClient : RestClient
    {

        private readonly AzureAuthenticationService authService = new AzureAuthenticationService();

        public RetryRestClient(string baseUrl) : base(baseUrl)
        {

        }

        public override IRestResponse Execute(IRestRequest request)
        {
            IRestResponse response = base.Execute(request);

            if (!HttpUtils.IsResponseValid(response))
            {
                authService.Login();
                ShimNewAuthHeader(request);

                response = base.Execute(request);
            }

            if (!HttpUtils.IsResponseValid(response))
            {
                return null;
            }

            return response;
        }

        // region Private Methods

        private void ShimNewAuthHeader(IRestRequest request)
        {
            string bearerToken = $"{MediaServicesConstants.Headers.Authorization.Item2} {TokenRepository.AccessToken}";
            Parameter authHeader = request.Parameters.Find(param =>
            {
                return param.Name == MediaServicesConstants.Headers.Authorization.Item1;
            });

            if (authHeader != null)
            {
                request.Parameters.Remove(authHeader);
            }

            request.AddHeader(MediaServicesConstants.Headers.Authorization.Item1, bearerToken);
        }

        // endregion
    }
}
