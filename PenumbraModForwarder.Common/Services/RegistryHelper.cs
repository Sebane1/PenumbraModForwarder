using Microsoft.Win32;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class RegistryHelper : IRegistryHelper
{
    private const string RegistryPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FFXIV_TexTools";
    
    private string GetRegistryValue(string keyValue)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryPath);
            return key?.GetValue(keyValue)?.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error accessing registry: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }
    
    public string GetTexToolsConsolePath()
    {
        var path = GetRegistryValue("InstallLocation");
        
        // Strip the path of ""
        if (path.StartsWith("\"") && path.EndsWith("\""))
        {
            path = path[1..^1];
        }
        
        // The path just returns the folder, we need to find ConsoleTools.exe which is at the location /path/FFXIV_TexTools/ConsoleTools.exe
        var combinedPath = Path.Combine(path, "FFXIV_TexTools", "ConsoleTools.exe");

        // Check if ConsoleTools.exe exists
        return File.Exists(combinedPath) ? combinedPath : string.Empty;
    }
}