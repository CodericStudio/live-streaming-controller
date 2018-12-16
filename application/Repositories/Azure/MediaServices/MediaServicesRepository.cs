using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using System.Collections.Generic;

namespace LiteralLifeChurch.LiveStreamingController.Repositories.Azure.MediaServices
{
    internal class MediaServicesRepository
    {
        public static List<ChannelModel> Channels = null;
        public static List<StreamingEndpointModel> Endpoints = null;
    }
}
