using System.Threading.Tasks;
using PenumbraModForwarder.UI.Models;

namespace PenumbraModForwarder.UI.Interfaces;

public interface INotificationService
{
    Task ShowNotification(string message, int durationSeconds = 4);
    void UpdateProgress(string title, string status, int progress);
    Task RemoveNotification(Notification notification);
}