using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services
{
    public class RegistryHelper : IRegistryHelper
    {
        private const string RegistryPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FFXIV_TexTools";
        private readonly IErrorWindowService _errorWindowService;
        private readonly ILogger<RegistryHelper> _logger;
        private readonly IConfigurationService _configurationService;

        public RegistryHelper(IErrorWindowService errorWindowService, ILogger<RegistryHelper> logger, IConfigurationService configurationService)
        {
            _errorWindowService = errorWindowService;
            _logger = logger;
            _configurationService = configurationService;
        }

        private string GetTexToolGetRegistryValue(string keyValue)
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(RegistryPath);
                return key?.GetValue(keyValue)?.ToString();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading registry");
                _errorWindowService.ShowError(e.ToString());
                return null;
            }
        }
        
        public string GetTexToolsConsolePath()
        {
            var path = GetTexToolGetRegistryValue("InstallLocation");

            // Strip the path of ""
            if (path.StartsWith("\"") && path.EndsWith("\""))
            {
                path = path[1..^1];
            }

            // The path just returns the folder, we need to find ConsoleTools.exe which is at the location /path/FFXIV_TexTools/ConsoleTools.exe
            var combinedPath = Path.Combine(path, "FFXIV_TexTools", "ConsoleTools.exe");
            
            return File.Exists(combinedPath) ? combinedPath : string.Empty;
        }

        public void SetTexToolsConsolePath()
        {
            try 
            {
                var combinedPath = GetTexToolsConsolePath();

                if (!string.IsNullOrEmpty(combinedPath))
                {
                    _configurationService.SetConfigValue((config, textToolPath) => config.TexToolPath = textToolPath, combinedPath);
                }
                else
                {
                    _logger.LogWarning("TexTools console path not found");
                    
                    _configurationService.SetConfigValue((config, textToolPath) => config.TexToolPath = textToolPath, string.Empty);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting TexTools console path");
                _errorWindowService.ShowError(e.ToString());
            }
        }
        
        public void CreateFileAssociation(string extension, string applicationPath)
        {
            try
            {
                using (var fileReg = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{extension}"))
                {
                    if (fileReg == null)
                        throw new InvalidOperationException($"Failed to create registry key for file extension '{extension}'.");

                    using (var commandKey = fileReg.CreateSubKey("shell\\open\\command"))
                    {
                        if (commandKey == null)
                            throw new InvalidOperationException($"Failed to create command registry key for file extension '{extension}'.");

                        commandKey.SetValue("", $"\"{applicationPath}\" \"%1\"");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating file association in registry");
                _errorWindowService.ShowError(e.ToString());
                throw;
            }
        }

        public void RemoveFileAssociation(string extension)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{extension}", false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error removing file association from registry");
                _errorWindowService.ShowError(e.ToString());
                throw;
            }
        }
        
        public void AddApplicationToStartup(string appName, string appPath)
        {
            try
            {
                using var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                registryKey?.SetValue(appName, $"\"{appPath}\"");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error adding application to startup");
                _errorWindowService.ShowError(e.ToString());
            }
        }

        public void RemoveApplicationFromStartup(string appName)
        {
            try
            {
                using var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                registryKey?.DeleteValue(appName, false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error removing application from startup");
                _errorWindowService.ShowError(e.ToString());
            }
        }
    }
}
