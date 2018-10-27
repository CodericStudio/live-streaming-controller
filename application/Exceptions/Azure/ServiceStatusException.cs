namespace LiteralLifeChurch.LiveStreamingController.Exceptions.Azure
{
    internal class ServiceStatusException : AzureException
    {
        public ServiceStatusException(string message) : base(message)
        {
        }
    }
}
