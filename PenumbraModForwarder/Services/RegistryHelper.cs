using Microsoft.Win32;

namespace PenumbraModForwarder.Services;

public static class RegistryHelper
{
    // I need to read the registry of HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\FFXIV_TexTools to get the InstallLocation
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
        return GetRegistryValue(RegistryPath, "InstallLocation");
    }

    public static string GetTexToolsConsolePath()
    {
        return GetInstallLocation() + @"\ConsoleTools.exe";
    }

}