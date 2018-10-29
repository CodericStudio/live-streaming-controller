using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteralLifeChurch.LiveStreamingController.Repositories.Azure.MediaServices
{
    internal class MediaServicesRepository : IRepository
    {
        public static List<string> Channels =>
            Environment.GetEnvironmentVariable(MediaServicesConstants.Channels).Split(',').ToList();

        public static List<string> Programs =>
            Environment.GetEnvironmentVariable(MediaServicesConstants.Programs).Split(',').ToList();

        public static List<string> StreamingEndpoints =>
            Environment.GetEnvironmentVariable(MediaServicesConstants.StreamingEndpoints).Split(',').ToList();
    }
}
