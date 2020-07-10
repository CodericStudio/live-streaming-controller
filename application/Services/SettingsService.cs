using LiteralLifeChurch.LiveStreamingController.Models;
using System;
using Windows.Storage;

namespace LiteralLifeChurch.LiveStreamingController.Services
{
    public class SettingsService : IService
    {
        private static readonly string ApiKey = "API_KEY";
        private static readonly string Host = "HOST";
        private static readonly string LiveEventNames = "LIVE_EVENT_NAMES";
        private static readonly string PollingInterval = "POLLING_INTERVAL";
        private static readonly string PollingIntervalEnabled = "POLLING_INTERVAL_ENABLED";
        private static readonly string StreamingEndpointName = "STREAMING_ENDPOINT_NAME";

        private readonly ApplicationDataContainer localSettings;

        public SettingsService()
        {
            localSettings = ApplicationData.Current.LocalSettings;
        }

        public bool AreSettingsPopulated
        {
            get
            {
                SettingsModel storage = Storage;

                bool isApiKeyPopulated = !string.IsNullOrWhiteSpace(storage.ApiKey);
                bool isHostPopulated = !string.IsNullOrWhiteSpace(storage.Host);
                bool isLiveEventsPopulated = !string.IsNullOrWhiteSpace(storage.LiveEventNames);
                bool isStreamingEndpointPopulated = !string.IsNullOrWhiteSpace(storage.StreamingEndpointName);

                return isApiKeyPopulated && isHostPopulated && isLiveEventsPopulated && isStreamingEndpointPopulated;
            }
        }

        public SettingsModel Storage
        {
            get
            {
                string apiKey = localSettings.Values.ContainsKey(ApiKey) ? localSettings.Values[ApiKey] as string : "";
                string host = localSettings.Values.ContainsKey(Host) ? localSettings.Values[Host] as string : "";
                string liveEventNames = localSettings.Values.ContainsKey(LiveEventNames) ? localSettings.Values[LiveEventNames] as string : "";
                int pollingInterval = localSettings.Values.ContainsKey(PollingInterval) ? Convert.ToInt32(localSettings.Values[PollingInterval]) : 1;
                bool pollingIntervalEnabled = localSettings.Values.ContainsKey(PollingIntervalEnabled) ? Convert.ToBoolean(localSettings.Values[PollingIntervalEnabled]) : false;
                string streamingEndpointName = localSettings.Values.ContainsKey(StreamingEndpointName) ? localSettings.Values[StreamingEndpointName] as string : "";

                return new SettingsModel
                {
                    ApiKey = apiKey,
                    Host = host,
                    LiveEventNames = liveEventNames,
                    PollingInterval = pollingInterval,
                    PollingIntervalEnabled = pollingIntervalEnabled,
                    StreamingEndpointName = streamingEndpointName
                };
            }

            set
            {
                localSettings.Values[ApiKey] = value.ApiKey;
                localSettings.Values[Host] = value.Host;
                localSettings.Values[LiveEventNames] = value.LiveEventNames;
                localSettings.Values[PollingInterval] = value.PollingInterval;
                localSettings.Values[PollingIntervalEnabled] = value.PollingIntervalEnabled;
                localSettings.Values[StreamingEndpointName] = value.StreamingEndpointName;
            }
        }
    }
}
