using System;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure.Authentication
{
    internal class AzureAuthenticationConstants
    {
        public static readonly Tuple<string, string> ContentType = new Tuple<string, string>("Content-Type", "application/x-www-form-urlencoded");
        public const string GrantType = "client_credentials";
        public const string Resources = "https://rest.media.azure.net";
    }
}
