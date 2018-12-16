using System;

namespace LiteralLifeChurch.LiveStreamingController.Services.Azure
{
    internal class MediaServicesConstants
    {
        internal class Conventions
        {
            internal class AccessPolicy
            {
                public const int DurationInMinutes = 525600; // Azure default, equivalent to 1 year
                public const string NamePrefix = "LiteralLifeChurch-LiveStreamingController-AccessPolicy-";
                public const int Permissions = 1; // Read-only flag
                public const string RegexSelector = @"LiteralLifeChurch\-LiveStreamingController\-AccessPolicy\-[a-fA-F0-9\-]{36}";
            }

            internal class Assets
            {
                public const string NamePrefix = "LiteralLifeChurch-LiveStreamingController-Asset-";
                public const string RegexSelector = @"LiteralLifeChurch\-LiveStreamingController\-Asset\-[a-fA-F0-9\-]{36}";
            }

            internal class Locators
            {
                public const string NamePrefix = "LiteralLifeChurch-LiveStreamingController-Locator-";
                public const string RegexSelector = @"LiteralLifeChurch\-LiveStreamingController\-Locator\-[a-fA-F0-9\-]{36}";
                public const int Type = 2; // On-Demand Origin
            }

            internal class Programs
            {
                public const string NamePrefix = "LiteralLifeChurch-LiveStreamingController-Program-";
                public const string RegexSelector = @"LiteralLifeChurch\-LiveStreamingController\-Program\-[a-fA-F0-9\-]{36}";
            }
        }

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
            public const string Path = "Path";
            public const string Status = "State"; // Azure calls it state, we call it status throughout the entire app
            public const string Value = "value";
        }

        internal class Paths
        {
            internal class AccessPolicy
            {
                public const string Create = "AccessPolicies";
            }

            internal class Assets
            {
                public const string Create = "Assets";
            }

            internal class Channels
            {
                public const string List = "Channels";
                public const string Start = "Channels('{0}')/Start";
            }

            internal class Locators
            {
                public const string Create = "Locators";
            }

            internal class Programs
            {
                public const string Create = "Programs";
                public const string List = "Programs";
                public const string Start = "Programs('{0}')/Start";
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
