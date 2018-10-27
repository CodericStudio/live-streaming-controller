namespace LiteralLifeChurch.LiveStreamingController.Exceptions.Azure
{
    internal class AzureAuthenticationException : AzureException
    {
        public AzureAuthenticationException(string message) : base(message)
        {
        }
    }
}
