namespace LiteralLifeChurch.LiveStreamingController.Exceptions
{
    public class StatusNotStableException : AppException
    {
        public StatusNotStableException() : base("The service status is stil settling to a stable state")
        {
        }
    }
}
