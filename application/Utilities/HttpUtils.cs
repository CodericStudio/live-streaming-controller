using RestSharp;
using System;
using System.Net;

namespace LiteralLifeChurch.LiveStreamingController.Utilities
{
    internal class HttpUtils
    {

        public static bool Is2xx(HttpStatusCode code)
        {
            int response = Convert.ToInt32(code);
            return 200 <= response && response <= 299;
        }

        public static bool IsResponseValid(IRestResponse response)
        {
            if (response == null || !Is2xx(response.StatusCode))
            {
                return false;
            }

            return true;
        }
    }
}
