using System.Threading.Tasks;

namespace PenumbraModForwarder.UI.Interfaces;

public interface INotificationService
{
    Task ShowNotification(string message);
    void UpdateProgress(string title, string status, int progress);
}