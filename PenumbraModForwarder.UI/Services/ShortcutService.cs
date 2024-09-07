using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.UI.Interfaces;
using WindowsShortcutFactory;

namespace PenumbraModForwarder.UI.Services;

public class ShortcutService : IShortcutService
{
    private readonly ILogger<ShortcutService> _logger;
    private readonly IErrorWindowService _errorWindowService;

    public ShortcutService(ILogger<ShortcutService> logger, IErrorWindowService errorWindowService)
    {
        _logger = logger;
        _errorWindowService = errorWindowService;
    }

    public void CreateShortcutInStartMenus()
    {
        try
        {
            // Define the path for the Start Menu Programs folder
            var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            
            var appFolderPath = Path.Combine(startMenuPath, "Programs", "Penumbra Mod Forwarder");
            var shortcutPath = Path.Combine(appFolderPath, "Penumbra Mod Forwarder.lnk");
            
            var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PenumbraModForwarder.exe");
            
            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
            }
            
            using var shortcut = new WindowsShortcut();
            shortcut.Path = appPath; 
            shortcut.WorkingDirectory = Path.GetDirectoryName(appPath);
            shortcut.Description = "Penumbra Mod Forwarder";

            // Save the shortcut inside the application folder
            shortcut.Save(shortcutPath);

            _logger.LogInformation("Created shortcut in start menu folder at: " + shortcutPath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create shortcut in start menu");
            _errorWindowService.ShowError(e.ToString());
        }
    }
}