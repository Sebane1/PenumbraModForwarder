namespace PenumbraModForwarder.Common.Interfaces;

public interface ITrayNotificationService : IDisposable
{
    void ShowNotification(string title, string message);
}