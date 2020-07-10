namespace LiteralLifeChurch.LiveStreamingController.Models
{
    public class SettingsModel : IModel
    {
        public string ApiKey { get; set; }

        public string Host { get; set; }

        public string LiveEventNames { get; set; }

        public int PollingInterval { get; set; }

        public bool PollingIntervalEnabled { get; set; }

        public string StreamingEndpointName { get; set; }
    }
}
