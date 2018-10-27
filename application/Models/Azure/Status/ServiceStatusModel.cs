using LiteralLifeChurch.LiveStreamingController.Enums.Azure;
using LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices;
using System.Collections.Generic;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Status
{
    internal class ServiceStatusModel : IStatusModel
    {
        public List<ChannelModel> Channels { get; set; }
        public List<EndpointModel> Endpoints { get; set; }
        public List<ProgramModel> Programs { get; set; }
        public StatusType Summary { get; set; }
    }
}
