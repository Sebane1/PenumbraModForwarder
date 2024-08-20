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
        private const int TimeoutMs = 5000;
        private static readonly HttpClient HttpClient;
        private static bool _warningShown;
        private readonly IErrorWindowService _errorWindowService;
        private readonly ILogger<PenumbraApi> _logger;
        private readonly ISystemTrayManager _systemTrayManager;

        static PenumbraApi()
        {
            HttpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(TimeoutMs)
            };
        }

        public PenumbraApi(ILogger<PenumbraApi> logger, IErrorWindowService errorWindowService, ISystemTrayManager systemTrayManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errorWindowService = errorWindowService;
            _systemTrayManager = systemTrayManager;
        }

        public async Task InstallAsync(string modPath)
        {
            var data = new ModInstallData(modPath);

            try
            {
                _logger.LogInformation("Sending install request for mod at {ModPath}", modPath);
                await PostAsync("/installmod", data);
                _logger.LogInformation("Install request sent successfully for mod at {ModPath}", modPath);
                _systemTrayManager.ShowNotification("Mod Installed", $"Mod installed successfully: {Path.GetFileName(modPath)}");
                
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to install mod at {ModPath}", modPath);
                throw; // Re-throw to allow higher-level handlers to process it
            }
        }

        private async Task PostAsync(string route, object content)
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
                }
                
                _logger.LogInformation("Successfully posted to {Route}", route);
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
            _logger.LogInformation("Posting request to {RequestUri} with content: {Content}", requestUri, json);
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
