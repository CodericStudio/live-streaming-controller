using LiteralLifeChurch.LiveStreamingController.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace LiteralLifeChurch.LiveStreamingController.Services
{
    public class NotificationService : IService
    {
        public void ShowNotification(NotificationContentModel model)
        {
            ToastContent content = new ToastContent
            {
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = model.Title
                            },
                            new AdaptiveText
                            {
                                Text = model.Message
                            }
                        }
                    }
                }
            };

            ToastNotification notification = new ToastNotification(content.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
