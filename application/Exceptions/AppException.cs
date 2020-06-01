using System;

namespace LiteralLifeChurch.LiveStreamingController.Exceptions
{
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message)
        {
        }
    }
}
