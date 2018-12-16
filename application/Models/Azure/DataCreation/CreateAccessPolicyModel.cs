namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.DataCreation
{
    internal class CreateAccessPolicyModel
    {
        public string DurationInMinutes { get; set; }
        public string Name { get; set; }
        public int Permissions { get; set; }
    }
}
