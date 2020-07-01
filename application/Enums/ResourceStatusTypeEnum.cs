using System.Runtime.Serialization;

namespace LiteralLifeChurch.LiveStreamingController.Enums
{
    public enum ResourceStatusTypeEnum
    {
        [EnumMember(Value = "stable")]
        Stable,

        [EnumMember(Value = "transient")]
        Transient
    }
}
