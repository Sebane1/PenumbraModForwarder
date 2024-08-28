using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Interfaces;
using PenumbraModForwarder.UI.Views;

namespace PenumbraModForwarder.UI.Services;

public class SystemTrayManager : ISystemTrayManager
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ILogger<SystemTrayManager> _logger;
    private readonly IErrorWindowService _errorWindowService;
    private readonly IConfigurationService _configurationService;
    private readonly IProcessHelperService _processHelperService;
    private readonly IResourceManager _resourceManager;
    
    private Icon _icon;

    public event Action OnExitRequested;

    public SystemTrayManager(ILogger<SystemTrayManager> logger, IErrorWindowService errorWindowService, IConfigurationService configurationService, IProcessHelperService processHelperService, IResourceManager resourceManager)
    {
        _logger = logger;
        _errorWindowService = errorWindowService;
        _configurationService = configurationService;
        _processHelperService = processHelperService;
        _resourceManager = resourceManager;
        _icon = _resourceManager.LoadIcon("PenumbraModForwarder.UI.Resources.PMFI.ico");
        _notifyIcon = new NotifyIcon
        {
            Icon = _icon,
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
        contextMenu.Items.Add("Open Configuration", null, (sender, args) =>
        {
            _logger.LogInformation("Opening configuration window.");
            var mainWindow = Application.OpenForms.OfType<MainWindow>().FirstOrDefault();

            if (mainWindow == null) return;
            
            if (mainWindow.WindowState == FormWindowState.Minimized)
            {
                mainWindow.WindowState = FormWindowState.Normal;
            }
                
            if (!mainWindow.Visible)
            {
                mainWindow.Show();
            }
                
            mainWindow.Activate();
        });
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        contextMenu.Items.Add(CreateDebuggingSubMenu());
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        contextMenu.Items.Add(CreateQuickLinksSubmenu());
        contextMenu.Items.Add(CreateResourcesSubmenu());
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        var donateButton = ColouredMenuItem("Donate", Color.Green);
        donateButton.Click += (sender, args) => _processHelperService.OpenDonate();
        contextMenu.Items.Add(donateButton);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        var exitButton = ColouredMenuItem("Exit", Color.Red);
        exitButton.Click += (sender, args) => TriggerExit();
        contextMenu.Items.Add(exitButton);
    }


    public void TriggerExit()
    {
        _logger.LogInformation("Exit triggered from SystemTrayManager.");
        OnExitRequested?.Invoke();
    }
    
    private static ToolStripMenuItem ColouredMenuItem(string text, Color colour)
    {
        var item = new ToolStripMenuItem(text);
        item.ForeColor = colour;
        return item;
    }

    private ToolStripMenuItem CreateDebuggingSubMenu()
    {
        var debugging = new ToolStripMenuItem("Debugging");
        debugging.DropDownItems.Add("Open Log Folder", null, (sender, args) => _processHelperService.OpenLogFolder());
        return debugging;
    }

    private ToolStripMenuItem CreateResourcesSubmenu()
    {
        var resources = new ToolStripMenuItem("Resources");
        resources.DropDownItems.Add("CrossGenPorting", null, (sender, args) => _processHelperService.CrossGenPorting());
        resources.DropDownItems.Add("Xiv Mod Resources", null, (sender, args) => _processHelperService.XivModResources());
        resources.DropDownItems.Add("TexTools Discord", null, (sender, args) => _processHelperService.TexToolsDiscord());
        resources.DropDownItems.Add("Sound and Texture Resources", null, (sender, args) => _processHelperService.SoundAndTextureResources());
        // resources.DropDownItems.Add("Pixelated Assistance", null, (sender, args) => _processHelperService.PixelatedAssistance());
        resources.DropDownItems.Add("Penumbra Resources", null, (sender, args) => _processHelperService.PenumbraResources());
        
        return resources;
    }
    
    private ToolStripMenuItem CreateQuickLinksSubmenu()
    {
        var quickLinks = new ToolStripMenuItem("Quick Links");
        quickLinks.DropDownItems.Add("Xiv Mod Archive", null, (sender, args) => _processHelperService.OpenXivArchive());
        quickLinks.DropDownItems.Add("Glamour Dresser", null, (sender, args) => _processHelperService.OpenGlamourDresser());
        quickLinks.DropDownItems.Add("Nexus Mods", null, (sender, args) => _processHelperService.OpenNexusMods());
        quickLinks.DropDownItems.Add("Aetherlink", null, (sender, args) => _processHelperService.OpenAetherLink());
        quickLinks.DropDownItems.Add("Heliosphere", null, (sender, args) => _processHelperService.OpenHelios());
        quickLinks.DropDownItems.Add("The Pretty Kitty Emporium", null, (sender, args) => _processHelperService.OpenPrettyKitty());
        
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
        Application.OpenForms.OfType<MainWindow>().FirstOrDefault()?.Activate();
    }
    
    public void Dispose()
    {
        _notifyIcon.Dispose();
    }
}