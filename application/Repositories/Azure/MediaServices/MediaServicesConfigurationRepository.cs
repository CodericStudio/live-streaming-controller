using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Repositories.Azure.MediaServices
{
    internal class MediaServicesConfigurationRepository : IRepository
    {
        public static List<string> Channels =>
            Environment.GetEnvironmentVariable(MediaServicesConfigurationConstants.Channels).Split(',').ToList();

        public static string ProgramArchiveWindowDuration =>
            Environment.GetEnvironmentVariable(MediaServicesConfigurationConstants.ProgramArchiveWindowLength);

        public static List<string> Programs =>
            Environment.GetEnvironmentVariable(MediaServicesConfigurationConstants.Programs).Split(',').ToList();

        public static List<string> StreamingEndpoints =>
            Environment.GetEnvironmentVariable(MediaServicesConfigurationConstants.StreamingEndpoints).Split(',').ToList();
    }
}
