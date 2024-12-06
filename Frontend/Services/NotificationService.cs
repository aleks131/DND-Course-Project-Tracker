using System;

namespace Frontend.Services
{
    public class NotificationService : INotificationService
    {
        public event Action<NotificationType, string>? OnNotification;

        public void ShowSuccess(string message)
        {
            Show(NotificationType.Success, message);
        }

        public void ShowError(string message)
        {
            Show(NotificationType.Error, message);
        }

        public void ShowWarning(string message)
        {
            Show(NotificationType.Warning, message);
        }

        public void ShowInfo(string message)
        {
            Show(NotificationType.Info, message);
        }

        public void Show(NotificationType type, string message)
        {
            OnNotification?.Invoke(type, message);
        }
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
}
