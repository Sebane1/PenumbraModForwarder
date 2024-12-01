using Microsoft.Win32;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.Common.Services;

public class RegistryHelper : IRegistryHelper
{
    /// <summary>
    /// Retrieves the TexTools installation location from the Windows Registry
    /// </summary>
    /// <returns>The installation path of TexTools</returns>
    /// <exception cref="Exception">Thrown when registry access fails</exception>
    public string GetTexToolRegistryValue()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryConsts.RegistryPath);
            var value = key?.GetValue("InstallLocation")?.ToString();
            
            if (string.IsNullOrEmpty(value))
            {
                Log.Warning("Registry value not found at {Path}", RegistryConsts.RegistryPath);
            }
            
            return value;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get registry value for {Path}", RegistryConsts.RegistryPath);
            throw new Exception($"Failed to get registry value for {RegistryConsts.RegistryPath}", e);
        }
    }
}