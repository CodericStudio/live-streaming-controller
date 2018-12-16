using System;

namespace LiteralLifeChurch.LiveStreamingController.Repositories.Firebase
{
    internal class FirebaseConfigurationRepository : IRepository
    {
        public static string AppSecret =>
            Environment.GetEnvironmentVariable(FirebaseConfigurationConstants.AppSecret);

        public static string AppUrl =>
            Environment.GetEnvironmentVariable(FirebaseConfigurationConstants.AppUrl);
    }
}
