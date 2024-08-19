using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.UI.Services;

public class NotificationService : ITrayNotificationService
{
    private readonly NotifyIcon _notifyIcon;
    private bool _disposed;
    private readonly ILogger<NotificationService> _logger;
    private readonly IErrorWindowService _errorWindowService;

    public NotificationService(NotifyIcon notifyIcon, ILogger<NotificationService> logger, IErrorWindowService errorWindowService)
    {
        _notifyIcon = notifyIcon;
        _logger = logger;
        _errorWindowService = errorWindowService;
        _notifyIcon.Visible = true;
    }

    public void ShowNotification(string title, string message)
    {
        try
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(3000);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show notification.");
            _errorWindowService.ShowError(ex.ToString());
        }
    }
    
    public void Dispose()
    {
        _notifyIcon.Dispose();
        GC.SuppressFinalize(this);
    }
}