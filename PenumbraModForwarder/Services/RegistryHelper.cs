using Microsoft.Win32;

namespace PenumbraModForwarder.Services;

public static class RegistryHelper
{
    private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\FFXIV_TexTools";

    private static string GetRegistryValue(string keyName, string valueName)
    {
        try
        {
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName);
            if (key == null)
            {
                return "";
            }

            return key.GetValue(valueName).ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading registry: {e.Message}");
            return "";
        }
    }

    private static string GetInstallLocation()
    {
        var installLocation = GetRegistryValue(RegistryPath, "InstallLocation");
        if (string.IsNullOrEmpty(installLocation))
        {
            Console.WriteLine("Error: InstallLocation not found in registry.");
        }
        return installLocation;
    }

    public static string GetTexToolsConsolePath()
    {
        var installLocation = GetInstallLocation();
        if (string.IsNullOrEmpty(installLocation))
        {
            return "";
        }
        return installLocation;
    }

}