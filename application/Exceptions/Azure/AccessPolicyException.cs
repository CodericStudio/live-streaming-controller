namespace LiteralLifeChurch.LiveStreamingController.Exceptions.Azure
{
    internal class AccessPolicyException : AzureException
    {
        public AccessPolicyException() : base("Could not create access policy")
        {
        }
    }
}
