namespace LiteralLifeChurch.LiveStreamingController.Exceptions.Azure
{
    internal class LocatorException : AzureException
    {
        public LocatorException() : base("Could not create locator")
        {
        }
    }
}
