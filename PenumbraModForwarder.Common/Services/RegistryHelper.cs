using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using System.Diagnostics;
using System.IO;

public class RegistryHelper : IRegistryHelper
{
    private readonly IErrorWindowService _errorWindowService;
    private readonly ILogger<RegistryHelper> _logger;

    private const string HKLMOpenCommandPath = @"SOFTWARE\Classes\Penumbra Modpack File\shell\open\command";
    private const string RegistryPath = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\FFXIV_TexTools";
    
    private string SaveRegistryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PenumbraModForwarder\Registry");

    public RegistryHelper(IErrorWindowService errorWindowService, ILogger<RegistryHelper> logger)
    {
        _errorWindowService = errorWindowService;
        _logger = logger;

        if (!Directory.Exists(SaveRegistryPath))
        {
            Directory.CreateDirectory(SaveRegistryPath);
        }
    }

    public string GetTexToolRegistryValue(string keyValue)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(RegistryPath);
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
            if (IsFileAssociationSetCorrectly(applicationPath))
            {
                _logger.LogInformation("The file association is already correctly set. No update needed.");
                return;
            }

            _logger.LogInformation("Creating file association in registry");

            var regFilePath = SaveRegistryPath + @"\file_associations.reg";  // Use a generic filename 
            var regFileContent = GenerateSetFileAssociationRegContent(extensions, applicationPath);

            File.WriteAllText(regFilePath, regFileContent);
            ExecuteRegFile(regFilePath);

            _logger.LogInformation("File association created successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating file association in registry");
            _errorWindowService.ShowError(e.ToString());
            throw;
        }
    }

    public void RemoveFileAssociation(IEnumerable<string> extensions)
    {
        try
        {
            _logger.LogInformation("Removing file association in registry");

            var regFilePath = SaveRegistryPath + @"\file_associations.reg";
            var regFileContent = GenerateRemoveFileAssociationRegContent(extensions);

            File.WriteAllText(regFilePath, regFileContent);
            ExecuteRegFile(regFilePath);

            _logger.LogInformation("File association removed successfully.");
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
            if (IsApplicationInStartup(appName))
            {
                _logger.LogInformation("Application is already present in startup.");
                return;
            }
            
            var regFilePath = SaveRegistryPath + @"\" + appName + ".reg";
            var regFileContent = GenerateAddToStartupRegContent(appName, appPath);

            File.WriteAllText(regFilePath, regFileContent);
            ExecuteRegFile(regFilePath);

            _logger.LogInformation("Application added to startup.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding application to startup");
            _errorWindowService.ShowError(e.ToString());
            throw;
        }
    }

    public void RemoveApplicationFromStartup(string appName)
    {
        try
        {
            if (!IsApplicationInStartup(appName))
            {
                _logger.LogInformation("Application is not present in startup.");
                return;
            }
            
            var regFilePath = SaveRegistryPath + @"\" + appName + ".reg";
            var regFileContent = GenerateRemoveFromStartupRegContent(appName);

            File.WriteAllText(regFilePath, regFileContent);
            ExecuteRegFile(regFilePath);

            _logger.LogInformation("Application removed from startup.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing application from startup");
            _errorWindowService.ShowError(e.ToString());
            throw;
        }
    }

    private void ExecuteRegFile(string regFilePath)
    {
        var processInfo = new ProcessStartInfo("regedit.exe", $"/s \"{regFilePath}\"")
        {
            UseShellExecute = true,
            Verb = "runas"
        };

        using var process = Process.Start(processInfo);
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to execute reg file: {regFilePath}. Process exited with code {process.ExitCode}.");
        }

        File.Delete(regFilePath);
    }

    private string GenerateSetFileAssociationRegContent(IEnumerable<string> extensions, string applicationPath)
    {
        var content = "";
        var escapedAppPath = applicationPath.Replace("\\", "\\\\").Replace("\"", "\\\"");

        content += "Windows Registry Editor Version 5.00";

        foreach (var extension in extensions)
        {
            content += $@"

[HKEY_CURRENT_USER\Software\Classes\.{extension}]
@=""PenumbraModpackFile""

[HKEY_CURRENT_USER\Software\Classes\PenumbraModpackFile\shell\open\command]
@=""{escapedAppPath}"" ""%1""

[HKEY_LOCAL_MACHINE\Software\Classes\Penumbra Modpack File\shell\open\command]
@=""{escapedAppPath}"" ""%1""

[HKEY_LOCAL_MACHINE\Software\Classes\Penumbra Modpack File\DefaultIcon]
@=""{applicationPath}"", 0""
";
        }

        return content;
    }

    private string GenerateRemoveFileAssociationRegContent(IEnumerable<string> extensions)
    {
        var content = "Windows Registry Editor Version 5.00";

        foreach (var extension in extensions)
        {
            content += $@"

[-HKEY_CURRENT_USER\Software\Classes\.{extension}]
[-HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Penumbra Modpack File]
";
        }

        return content;
    }

    private string GenerateAddToStartupRegContent(string appName, string appPath)
    {
        var escapedAppPath = appPath.Replace("\\", "\\\\").Replace("\"", "\\\"");

        return $@"Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run]
""{appName}""=""{escapedAppPath}""
";
    }

    private string GenerateRemoveFromStartupRegContent(string appName)
    {
        return $@"Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run]
""{appName}""=-
";
    }
    
    private bool IsApplicationInStartup(string appName)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
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
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(HKLMOpenCommandPath);
            var currentValue = key?.GetValue("")?.ToString();
            var expectedValue = $"\"{applicationPath}\" \"%1\"";
            return currentValue == expectedValue;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error checking registry for existing file association");
            return false;
        }
    }
}
