using Windows.UI;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Status
{
    internal class ServiceStatusDesignUpdateModel : IStatusModel
    {
        public string ButtonTextResource { get; set; }
        public Color StatusTextColor { get; set; }
        public string StatusTextResource { get; set; }
    }
}
