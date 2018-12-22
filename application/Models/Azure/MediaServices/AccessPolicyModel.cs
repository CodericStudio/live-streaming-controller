namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices
{
    internal class AccessPolicyModel : IMediaServicesModel
    {
        public int DurationInMinutes { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Permissions { get; set; }
    }
}
