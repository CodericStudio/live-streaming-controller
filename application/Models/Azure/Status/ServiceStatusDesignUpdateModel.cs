using Windows.UI;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Status
{
    internal class ServiceStatusDesignUpdateModel : IStatusModel
    {

        public Color StatusTextColor { get; set; }
        public string StatusTextResource { get; set; }
    }
}
