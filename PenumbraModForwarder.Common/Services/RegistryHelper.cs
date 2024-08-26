using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PenumbraModForwarder.Common.Interfaces;

public class RegistryHelper : IRegistryHelper
{
    private readonly IErrorWindowService _errorWindowService;
    private readonly ILogger<RegistryHelper> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly IAdminService _adminService;

    private const string HKLMOpenCommandPath = @"SOFTWARE\Classes\Penumbra Modpack File\shell\open\command";
    private const string HKLMEditCommandPath = @"SOFTWARE\Classes\Penumbra Modpack File\shell\edit\command";
    private const string HKLMDefaultIconPath = @"SOFTWARE\Classes\Penumbra Modpack File\DefaultIcon";
    private const string RegistryPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FFXIV_TexTools";

    public RegistryHelper(IErrorWindowService errorWindowService, ILogger<RegistryHelper> logger, IConfigurationService configurationService, IAdminService adminService)
    {
        _errorWindowService = errorWindowService;
        _logger = logger;
        _configurationService = configurationService;
        _adminService = adminService;
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

    public void CreateFileAssociation(string extension, string applicationPath)
    {
        try
        {
            if (IsFileAssociationSetCorrectly(applicationPath))
            {
                _logger.LogInformation("The file association is already correctly set. No update needed.");
                return;
            }

            if (!_adminService.IsAdmin() && !IsRestartedWithAdminArg())
            {
                _logger.LogWarning("Admin privileges are required to set the file association.");
                _adminService.PromptForAdminRestart();
                return;
            }

            _logger.LogInformation("Creating file association in registry");
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

            UpdateGlobalFileAssociation(applicationPath);

            _logger.LogInformation("File association created successfully.");
            _adminService.PromptForUserRestart();
            Environment.Exit(0);
        }
        catch (UnauthorizedAccessException)
        {
            _adminService.PromptForAdminRestart();
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
            if (!_adminService.IsAdmin() && !IsRestartedWithAdminArg())
            {
                _logger.LogWarning("Admin privileges are required to remove the file association.");
                _adminService.PromptForAdminRestart();
                return;
            }

            Registry.CurrentUser.DeleteSubKeyTree($"Software\\Classes\\{extension}", false);
            RemoveGlobalFileAssociation();

            _logger.LogInformation("File association removed successfully.");
            _adminService.PromptForUserRestart();
            Environment.Exit(0);
        }
        catch (UnauthorizedAccessException)
        {
            _adminService.PromptForAdminRestart();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing file association from registry");
            _errorWindowService.ShowError($"Error removing file association from registry\nIf this message references something about insufficient permissions you can ignore this message. Error: {e.Message}");
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

    private void UpdateGlobalFileAssociation(string applicationPath)
    {
        try
        {
            using (var openKey = Registry.LocalMachine.CreateSubKey(HKLMOpenCommandPath))
            {
                if (openKey == null)
                    throw new InvalidOperationException("Failed to create or open the 'open' command registry key.");

                openKey.SetValue("", $"\"{applicationPath}\" \"%1\"");
            }

            using (var editKey = Registry.LocalMachine.OpenSubKey(HKLMEditCommandPath, writable: true))
            {
                if (editKey != null)
                {
                    Registry.LocalMachine.DeleteSubKeyTree(HKLMEditCommandPath);
                }
            }

            using (var iconKey = Registry.LocalMachine.CreateSubKey(HKLMDefaultIconPath))
            {
                if (iconKey != null)
                {
                    iconKey.SetValue("", $"\"{applicationPath}\",0");
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating global file association in registry");
            _errorWindowService.ShowError(e.ToString());
            throw;
        }
    }

    private void RemoveGlobalFileAssociation()
    {
        try
        {
            Registry.LocalMachine.DeleteSubKeyTree(@"SOFTWARE\Classes\Penumbra Modpack File", false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing global file association from registry");
            _errorWindowService.ShowError(e.ToString());
            throw;
        }
    }

    private bool IsRestartedWithAdminArg()
    {
        return Environment.GetCommandLineArgs().Contains("--admin");
    }

    private bool IsFileAssociationSetCorrectly(string applicationPath)
    {
        try
        {
            using (var key = Registry.LocalMachine.OpenSubKey(HKLMOpenCommandPath))
            {
                var currentValue = key?.GetValue("")?.ToString();
                var expectedValue = $"\"{applicationPath}\" \"%1\"";
                return currentValue == expectedValue;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error checking registry for existing file association");
            return false;
        }
    }
}
