namespace LiteralLifeChurch.LiveStreamingController.Exceptions.Azure
{
    internal class ProgramException : AzureException
    {
        public ProgramException() : base("Could not create program")
        {
        }
    }
}
