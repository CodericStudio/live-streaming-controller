using System.Windows.Media;

namespace LiteralLifeChurch.LiveStreamingController.Models.Azure.Status
{
    internal class ServiceStatusDesignUpdateModel : IStatusModel
    {

        public string StatusText { get; set; }
        public SolidColorBrush StatusTextColor { get; set; }
    }
}
