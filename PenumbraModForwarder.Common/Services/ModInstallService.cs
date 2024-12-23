using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
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

    public ModInstallService(HttpClient httpClient, IStatisticService statisticService)
    {
        _httpClient = httpClient;
        _statisticService = statisticService;
        _logger = Log.ForContext<ModInstallService>();
    }

    public async Task<bool> InstallModAsync(string path)
    {
        var modData = new ModInstallData(path);
        var json = JsonConvert.SerializeObject(modData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        _logger.Debug("Sending POST request to {Url}", new Uri(_httpClient.BaseAddress, "installmod"));
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
}