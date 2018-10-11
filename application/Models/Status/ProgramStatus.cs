using LiteralLifeChurch.LiveStreamingController.Enums;

namespace LiteralLifeChurch.LiveStreamingController.Models
{
    public class ProgramStatus : IStatus
    {
        public string Name { get; set; }
        public StatusType Status { get; set; }
    }
}
