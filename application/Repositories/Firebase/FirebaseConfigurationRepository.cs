using System;

namespace LiteralLifeChurch.LiveStreamingController.Repositories.Firebase
{
    internal class FirebaseConfigurationRepository : IRepository
    {
        public static string CredentialString =>
            Environment.GetEnvironmentVariable(FirebaseConfigurationConstants.CredentialString);

        public static string ProjectId =>
            Environment.GetEnvironmentVariable(FirebaseConfigurationConstants.ProjectId);
    }
}
