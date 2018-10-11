using LiteralLifeChurch.LiveStreamingController.Enums;

namespace LiteralLifeChurch.LiveStreamingController.Models
{
    public class EndpointStatus : IStatus
    {
        public string Name { get; set; }
        public StatusType Status { get; set; }
    }
}
