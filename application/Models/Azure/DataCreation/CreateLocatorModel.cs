namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.DataCreation
{
    internal class CreateLocatorModel : IDataCreationModel
    {
        public string AccessPolicyId { get; set; }
        public string AssetId { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }
}
