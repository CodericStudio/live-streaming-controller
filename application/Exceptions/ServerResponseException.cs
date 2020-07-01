namespace LiteralLifeChurch.LiveStreamingController.Exceptions
{
    public class ServerResponseException : AppException
    {
        public ServerResponseException() : base("Invalid response received from the server. Check your internet connection and ensure your settings are correct on the settings page.")
        {
        }
    }
}
