using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Views;

namespace PenumbraModForwarder.UI.Services;

public class SystemTrayManager : ISystemTrayManager
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ILogger<SystemTrayManager> _logger;
    private readonly IErrorWindowService _errorWindowService;
    private readonly IConfigurationService _configurationService;
    private readonly IProcessHelperService _processHelperService;

    public SystemTrayManager(ILogger<SystemTrayManager> logger, IErrorWindowService errorWindowService, IConfigurationService configurationService, IProcessHelperService processHelperService)
    {
        _logger = logger;
        _errorWindowService = errorWindowService;
        _configurationService = configurationService;
        _processHelperService = processHelperService;
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            Visible = true,
            Text = "Penumbra Mod Fowarder",
        };

        var contextMenu = new ContextMenuStrip();
        AddItemsToContextMenu(contextMenu);
        _notifyIcon.ContextMenuStrip = contextMenu;
        
        _notifyIcon.DoubleClick += OnTrayIconDoubleClick;
    }
    
    private void AddItemsToContextMenu(ContextMenuStrip contextMenu)
    {
        contextMenu.Items.Add("Show", null, (sender, args) => Application.OpenForms.OfType<MainWindow>().FirstOrDefault()?.Show());
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        contextMenu.Items.Add(CreateQuickLinksSubmenu());
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        contextMenu.Items.Add("Exit", null, (sender, args) => Application.Exit());
    }

    private ToolStripMenuItem CreateQuickLinksSubmenu()
    {
        var quickLinks = new ToolStripMenuItem("Quick Links");
        quickLinks.DropDownItems.Add("Xiv Mod Archive", null, (sender, args) => _processHelperService.OpenXivArchive());
        quickLinks.DropDownItems.Add("Glamour Dressor", null, (sender, args) => _processHelperService.OpenGlamourDresser());
        
        return quickLinks;
    }
    
    public void ShowNotification(string title, string message)
    {
        if (!_configurationService.GetConfigValue(o => o.NotificationEnabled)) return;
        try 
        {
            _logger.LogInformation("Showing notification: {title} - {message}", title, message);
            _notifyIcon.ShowBalloonTip(5000, title, message, ToolTipIcon.Info);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to show notification.");
            _errorWindowService.ShowError(e.ToString());
        }
    }
    
    private void OnTrayIconDoubleClick(object sender, EventArgs e)
    {
        Application.OpenForms.OfType<MainWindow>().FirstOrDefault()?.Show();
    }
    
    public void Dispose()
    {
        _notifyIcon.Dispose();
    }
}