using LiteralLifeChurch.LiveStreamingController.Exceptions.Azure;
using LiteralLifeChurch.LiveStreamingController.Repositories.Azure.Authentication;
using LiteralLifeChurch.LiveStreamingController.Utilities;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Web;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.Authentication
{
    internal class AzureAuthenticationService
    {
        public void Login()
        {
            RestClient client = new RestClient(AuthenticationConfigurationRepository.OAuthEndpoint);
            RestRequest request = CreateLoginRequest();
            IRestResponse response = client.Execute(request);

            if (!HttpUtils.IsResponseValid(response))
            {
                new AzureAuthenticationException("Invalid response during authentication");
            }

            JObject json = JObject.Parse(response.Content);
            string accessToken = json.SelectToken("access_token").Value<string>().Trim();

            if (string.IsNullOrEmpty(accessToken))
            {
                new AzureAuthenticationException("Payload did not contain an access_token");
            }

            TokenRepository.AccessToken = accessToken;
        }

        private RestRequest CreateLoginRequest()
        {
            string encodedGrantType = HttpUtility.UrlEncode(AzureAuthenticationConstants.GrantType);
            string encodedResource = HttpUtility.UrlEncode(AzureAuthenticationConstants.Resources);

            string encodedId = HttpUtility.UrlEncode(AuthenticationConfigurationRepository.ClientId);
            string encodedSecret = HttpUtility.UrlEncode(AuthenticationConfigurationRepository.ClientSecret);

            string body = $"grant_type={encodedGrantType}&client_id={encodedId}&client_secret={encodedSecret}&resource={encodedResource}";

            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader(AzureAuthenticationConstants.ContentType.Item1, AzureAuthenticationConstants.ContentType.Item2);
            request.AddParameter(AzureAuthenticationConstants.ContentType.Item2, body, ParameterType.RequestBody);

            return request;
        }
    }
}
