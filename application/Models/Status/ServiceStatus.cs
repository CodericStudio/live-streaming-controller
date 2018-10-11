using LiteralLifeChurch.LiveStreamingController.Enums;
using System.Collections.Generic;

namespace LiteralLifeChurch.LiveStreamingController.Models
{
    public class ServiceStatus : IStatus
    {
        public List<ChannelStatus> Channels { get; set; }
        public List<EndpointStatus> Endpoints { get; set; }
        public List<ProgramStatus> Programs { get; set; }
        public StatusType Summary { get; set; }
    }
}
