using Microsoft.WindowsAzure.MediaServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Services
{
    public class AuthenticationService
    {

        // region Environment Variable Wrappers

        public static List<string> Channels {
            get { return Environment.GetEnvironmentVariable(AuthenticationConstants.Channels).Split(',').ToList(); }
        }

        public static string ClientId {
            get { return Environment.GetEnvironmentVariable(AuthenticationConstants.ClientId); }
        }

        public static string ClientSecret {
            get { return Environment.GetEnvironmentVariable(AuthenticationConstants.ClientSecret); }
        }

        public static List<string> Programs {
            get { return Environment.GetEnvironmentVariable(AuthenticationConstants.Programs).Split(',').ToList(); }
        }

        public static string RestApiEndpoint {
            get { return Environment.GetEnvironmentVariable(AuthenticationConstants.RestApiEndpoint); }
        }

        public static List<string> StreamingEndpoints {
            get { return Environment.GetEnvironmentVariable(AuthenticationConstants.StreamingEndpoints).Split(',').ToList(); }
        }

        public static string TenantId {
            get { return Environment.GetEnvironmentVariable(AuthenticationConstants.TenantId); }
        }

        // endregion

        public CloudMediaContext GetContext()
        {
            AzureAdClientSymmetricKey key = new AzureAdClientSymmetricKey(ClientId, ClientSecret);
            AzureAdTokenCredentials credentials = new AzureAdTokenCredentials(TenantId, key, AzureEnvironments.AzureCloudEnvironment);
            AzureAdTokenProvider token = new AzureAdTokenProvider(credentials);
            Uri restApiUrl = new Uri(RestApiEndpoint);

            return new CloudMediaContext(restApiUrl, token);
        }
    }
}
