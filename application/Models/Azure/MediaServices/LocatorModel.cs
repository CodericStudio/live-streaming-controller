namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.MediaServices
{
    internal class LocatorModel : IMediaServicesModel
    {
        public string AccessPolicyId { get; set; }
        public string AssetId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }
}
