namespace LiteralLifeChurch.LiveStreamingController.Exceptions.Azure
{
    internal class AssetException : AzureException
    {
        public AssetException() : base("Could not create asset")
        {
        }
    }
}
