using System.Windows.Media;

namespace LiteralLifeChurch.LiveStreamingController.Models.Status
{
    public class ServiceStatusDesignUpdateModel : IStatus
    {

        public string StatusText { get; set; }
        public SolidColorBrush StatusTextColor { get; set; }
    }
}
