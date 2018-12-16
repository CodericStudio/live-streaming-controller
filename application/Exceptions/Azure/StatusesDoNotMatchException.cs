namespace LiteralLifeChurch.LiveStreamingController.Exceptions.Azure
{
    internal class StatusesDoNotMatchException : AzureException
    {
        public StatusesDoNotMatchException() : base("Statuses do not match")
        {
        }
    }
}
