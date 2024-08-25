using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services
{
    public class RegistryHelper : IRegistryHelper
    {
        private readonly IErrorWindowService _errorWindowService;
        private readonly ILogger<RegistryHelper> _logger;
        private readonly IConfigurationService _configurationService;
        private const string RegistryPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FFXIV_TexTools";

        public RegistryHelper(IErrorWindowService errorWindowService, ILogger<RegistryHelper> logger, IConfigurationService configurationService)
        {
            _errorWindowService = errorWindowService;
            _logger = logger;
            _configurationService = configurationService;
        }
        
        public string GetTexToolGetRegistryValue(string keyValue)
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
