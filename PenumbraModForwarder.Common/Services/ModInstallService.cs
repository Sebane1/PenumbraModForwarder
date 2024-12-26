using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Exceptions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
using Serilog;

namespace PenumbraModForwarder.Common.Services;

public class ModInstallService : IModInstallService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly IStatisticService _statisticService;
    private readonly IPenumbraService _penumbraService;

    public ModInstallService(
        HttpClient httpClient,
        IStatisticService statisticService,
        IPenumbraService penumbraService)
    {
        _httpClient = httpClient;
        _statisticService = statisticService;
        _penumbraService = penumbraService;
        _logger = Log.ForContext<ModInstallService>();
    }

    public async Task<bool> InstallModAsync(string path)
    {
        var extension = Path.GetExtension(path);
        if (extension.Equals(".pmp", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                _logger.Debug("Using PenumbraService for .pmp mod: {Path}", path);
                
                _penumbraService.InitializePenumbraPath();
                
                var installedFolderPath = _penumbraService.InstallMod(path);

                // Reload the mod using the final installation folder
                var modName = Path.GetFileName(installedFolderPath);
                await ReloadModAsync(installedFolderPath, modName);
                
                var fileName = Path.GetFileName(path);
                await _statisticService.RecordModInstallationAsync(fileName);

                _logger.Information("Mod installed successfully from path '{Path}' using PenumbraService", path);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error installing .pmp mod from path '{Path}'", path);
                throw new ModInstallException($"Failed to install .pmp mod from path '{path}'.", ex);
            }
        }

        // Fallback for other file types
        var modData = new ModInstallData(path);
        var json = JsonConvert.SerializeObject(modData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        _logger.Debug("Sending POST request to {Url}", new Uri(_httpClient.BaseAddress!, "installmod"));

        try
        {
            var response = await _httpClient.PostAsync("installmod", content);
            response.EnsureSuccessStatusCode();

            _logger.Information("Mod installed successfully from path '{Path}'", path);

            var fileName = Path.GetFileName(path);
            await _statisticService.RecordModInstallationAsync(fileName);

            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.Error(ex, "HTTP request exception while installing mod from path '{Path}'", path);
            throw new ModInstallException($"Failed to install mod from path '{path}'.", ex);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected exception while installing mod from path '{Path}'", path);
            throw new ModInstallException($"An unexpected error occurred while installing mod from path '{path}'.", ex);
        }
    }

    /// <summary>
    /// Posts to the /reloadmod endpoint in Penumbra to reload a mod by folder path and mod name.
    /// </summary>
    private async Task ReloadModAsync(string modFolder, string modName)
    {
        var data = new ModReloadData(modFolder, modName);
        var body = JsonConvert.SerializeObject(data);
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        _logger.Debug("Posting to /reloadmod for folder '{ModFolder}', name '{ModName}'", modFolder, modName);
        var response = await _httpClient.PostAsync("reloadmod", content);
        response.EnsureSuccessStatusCode();
        
        await Task.Delay(200);

        _logger.Information("Successfully reloaded Penumbra mod at '{ModFolder}' with name '{ModName}'", modFolder, modName);
    }
}