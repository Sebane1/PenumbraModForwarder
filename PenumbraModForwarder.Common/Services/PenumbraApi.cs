using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services
{
    public class PenumbraApi : IPenumbraApi
    {
        private const string BaseUrl = "http://localhost:42069/api";
        private HttpClient HttpClient;
        private static bool _warningShown;
        private readonly IErrorWindowService _errorWindowService;
        private readonly ILogger<PenumbraApi> _logger;
        private readonly ISystemTrayManager _systemTrayManager;
        private readonly IConfigurationService _configurationService;

        public PenumbraApi(ILogger<PenumbraApi> logger, IErrorWindowService errorWindowService, ISystemTrayManager systemTrayManager, IConfigurationService configurationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorWindowService = errorWindowService;
            _systemTrayManager = systemTrayManager;
            _configurationService = configurationService;
            
            HttpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(_configurationService.GetConfigValue(o => o.AdvancedOptions.PenumbraTimeOut))
            };
        }

        public async Task<bool> InstallAsync(string modPath)
        {
            var data = new ModInstallData(modPath);

            try
            {
                _logger.LogDebug("Sending install request for mod at {ModPath}", modPath);
                var result = await PostAsync("/installmod", data);
                
                if (result)
                {
                    _logger.LogDebug("Install request sent successfully for mod at {ModPath}", modPath);
                    _systemTrayManager.ShowNotification("Mod Installed", $"Mod installed successfully: {Path.GetFileName(modPath)}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to install mod at {ModPath}", modPath);
                throw; // Re-throw to allow higher-level handlers to process it
            }

            return false;
        }
        
        // TODO: Create a function to poll the /reloadmod endpoint to check if our mod has been installed correctly, give it a retry count of 15
        private async Task<bool> IsModInstalledAsync(string modPath, string modName)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> PostAsync(string route, object content)
        {
            try
            {
                var response = await PostRequestAsync(route, content);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("HTTP request error: {StatusCode} - {ReasonPhrase}. Response body: {ResponseBody}", 
                        response.StatusCode, response.ReasonPhrase, responseBody);
                    response.EnsureSuccessStatusCode();
                    return false;
                }
                
                _logger.LogDebug("Successfully posted to {Route}", route);
                return true;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while posting to {Route}", route);
                HandleWarning(httpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while posting to {Route}", route);
                HandleWarning(ex);
            }
            
            return false;
        }

        private async Task<HttpResponseMessage> PostRequestAsync(string route, object content)
        {
            // Ensure that the route starts with "/api"
            if (!route.StartsWith("/api"))
                route = "/api" + route;

            var json = JsonConvert.SerializeObject(content);
            var byteContent = new ByteArrayContent(Encoding.UTF8.GetBytes(json))
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            };

            var requestUri = new Uri(new Uri(BaseUrl), route);
            _logger.LogDebug("Posting request to {RequestUri} with content: {Content}", requestUri, json);
            return await HttpClient.PostAsync(requestUri, byteContent);
        }


        private void HandleWarning(Exception ex)
        {
            if (!_warningShown)
            {
                _logger.LogWarning(ex, "Error communicating with Penumbra. Please ensure the HTTP API is enabled in Penumbra under 'Settings -> Advanced'.");
                _errorWindowService.ShowError(ex.ToString());
                _warningShown = true;
            }
        }

        private record ModInstallData(string Path)
        {
            public ModInstallData() : this(string.Empty) { }
        }
    }
}
