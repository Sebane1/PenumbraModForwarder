using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PenumbraModForwarder.Common.Interfaces;
using System;
using System.IO;

public class RegistryHelper : IRegistryHelper
{
    private readonly IErrorWindowService _errorWindowService;
    private readonly ILogger<RegistryHelper> _logger;
    private const string HKLMOpenCommandPath = @"SOFTWARE\Classes\PenumbraModpackFile\shell\open\command";
    private const string HKCUClassesPath = @"Software\Classes\";
    private const string RegistryPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FFXIV_TexTools";

    public RegistryHelper(IErrorWindowService errorWindowService, ILogger<RegistryHelper> logger)
    {
        _errorWindowService = errorWindowService;
        _logger = logger;
    }
    
    public string GetTexToolRegistryValue(string keyValue)
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
    
    public void CreateFileAssociation(IEnumerable<string> extensions, string applicationPath)
    {
        try
        {
            foreach (var extension in extensions)
            {
                var extensionKeyPath = $@"{HKCUClassesPath}.{extension}";
                using (var key = Registry.CurrentUser.CreateSubKey(extensionKeyPath))
                {
                    key?.SetValue("", "PenumbraModpackFile");
                }
            }

            using (var iconKey = Registry.CurrentUser.CreateSubKey($@"{HKCUClassesPath}PenumbraModpackFile\DefaultIcon"))
            {
                iconKey?.SetValue("", $"{applicationPath},0");
            }

            using (var commandKey = Registry.CurrentUser.CreateSubKey($@"{HKCUClassesPath}PenumbraModpackFile\shell\open\command"))
            {
                commandKey?.SetValue("", $"\"{applicationPath}\" \"%1\"");
            }

            _logger.LogInformation("File association created successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating file association in registry");
            _errorWindowService.ShowError(e.ToString());
        }
    }
    
    public void RemoveFileAssociation(IEnumerable<string> extensions)
    {
        try
        {
            foreach (var extension in extensions)
            {
                var extensionKeyPath = $@"{HKCUClassesPath}.{extension}";
                Registry.CurrentUser.DeleteSubKeyTree(extensionKeyPath, false);
            }

            Registry.CurrentUser.DeleteSubKeyTree($@"{HKCUClassesPath}PenumbraModpackFile", false);

            _logger.LogInformation("File association removed successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing file association from registry");
            _errorWindowService.ShowError(e.ToString());
        }
    }
    
    public void AddApplicationToStartup(string appName, string appPath)
    {
        try
        {
            using var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            registryKey?.SetValue(appName, $"\"{appPath}\"");

            _logger.LogInformation("Application added to startup.");
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

            _logger.LogInformation("Application removed from startup.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing application from startup");
            _errorWindowService.ShowError(e.ToString());
        }
    }
    
    private bool IsApplicationInStartup(string appName)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            var value = key?.GetValue(appName);
            return value != null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error checking if application is in startup");
            return false;
        }
    }
    
    private bool IsFileAssociationSetCorrectly(string applicationPath)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(HKLMOpenCommandPath);
            if (key == null)
            {
                _logger.LogWarning($"Registry key not found: {HKLMOpenCommandPath}");
                return false;
            }

            var currentValue = key.GetValue("")?.ToString();
            if (currentValue == null)
            {
                _logger.LogWarning($"Registry value not found at path: {HKLMOpenCommandPath}");
                return false;
            }

            currentValue = Environment.ExpandEnvironmentVariables(currentValue);
            var expectedValue = $"\"{applicationPath}\" \"%1\"";
            return string.Equals(currentValue, expectedValue, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error checking registry for existing file association");
            return false;
        }
    }
}
