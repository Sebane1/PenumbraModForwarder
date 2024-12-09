using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Exceptions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Services;

public class ModInstallService : IModInstallService
{
    private readonly HttpClient _httpClient;

    public ModInstallService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task InstallModAsync(string path)
    {
        var modData = new ModInstallData(path);
        
        var json = JsonConvert.SerializeObject(modData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var requestUri = $"{ApiConsts.BaseApiUrl}/installMod";
            var response = await _httpClient.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();
        }

        catch (HttpRequestException ex)
        {
            throw new ModInstallException($"Failed to install mod from path '{path}'.", ex);
        }

        catch (Exception ex)
        {
            throw new ModInstallException($"An unexpected error occurred while installing mod from path '{path}'.", ex);
        }
    }
}