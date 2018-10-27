using System;

namespace LiteralLifeChurch.LiveStreamingController.Repositories.Azure.Authentication
{
    internal class AuthenticationConfigurationRepository : IRepository
    {
        public static string ClientId =>
            Environment.GetEnvironmentVariable(AuthenticationConfigurationConstants.ClientId);

        public static string ClientSecret =>
            Environment.GetEnvironmentVariable(AuthenticationConfigurationConstants.ClientSecret);

        public static string OAuthEndpoint =>
            Environment.GetEnvironmentVariable(AuthenticationConfigurationConstants.OAuthEndpoint);

        public static string RestApiEndpoint =>
            Environment.GetEnvironmentVariable(AuthenticationConfigurationConstants.RestApiEndpoint);

        public static string TenantId =>
            Environment.GetEnvironmentVariable(AuthenticationConfigurationConstants.TenantId);
    }
}
