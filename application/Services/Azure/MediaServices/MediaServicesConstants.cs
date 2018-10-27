using System;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure
{
    internal class MediaServicesConstants
    {
        internal class Headers
        {
            public static readonly Tuple<string, string> AcceptHeader = new Tuple<string, string>("Accept", "application/json");
            public static readonly Tuple<string, string> Authorization = new Tuple<string, string>("Authorization", "Bearer");
            public static readonly Tuple<string, string> MsVersionHeader = new Tuple<string, string>("x-ms-version", "2.15");
        }

        internal class Json
        {
            public const string Id = "Id";
            public const string Name = "Name";
            public const string Status = "State"; // Azure calls it state, we call it status throughout the entire app
            public const string Value = "value";
        }

        internal class Paths
        {
            internal class Channels
            {
                public const string List = "Channels";
            }

            internal class Programs
            {
                public const string List = "Programs";
            }

            internal class StreamingEndpoints
            {
                public const string List = "StreamingEndpoints";
            }
        }

        internal class Statuses
        {
            public const string Running = "running";
            public const string Scaling = "scaling";
            public const string Starting = "starting";
        }
    }
}
