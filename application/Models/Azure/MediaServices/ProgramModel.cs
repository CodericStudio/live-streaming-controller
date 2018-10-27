using LiteralLifeChurch.LiveStreamingController.Enums.Azure;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices
{
    internal class ProgramModel : IMediaServicesModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public StatusType Status { get; set; }
    }
}
