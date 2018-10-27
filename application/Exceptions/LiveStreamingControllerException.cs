using System;

namespace LiteralLifeChurch.LiveStreamingController.Exceptions
{
    internal class LiveStreamingControllerException : Exception
    {
        public LiveStreamingControllerException(string message) : base(message)
        {
        }
    }
}
