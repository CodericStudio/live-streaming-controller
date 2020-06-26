namespace LiteralLifeChurch.LiveStreamingController.Models.Settings
{
    public class SettingsModel : ISettingsModel
    {
        public string ApiKey { get; set; }

        public string Host { get; set; }

        public string LiveEventNames { get; set; }

        public string StreamingEndpointName { get; set; }
    }
}
