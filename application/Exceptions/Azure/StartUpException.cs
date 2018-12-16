namespace LiteralLifeChurch.LiveStreamingController.Exceptions.Azure
{
    internal class StartUpException : AzureException
    {
        public StartUpException(string message) : base(message)
        {
        }
    }
}
