using System.Reflection;
using Newtonsoft.Json.Linq;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.Common.Services;

public class UpdateService
{
    private readonly IConfigurationService _configurationService;
    private readonly IFileStorage _fileStorage;
    private readonly HttpClient _httpClient;

    public UpdateService(IConfigurationService configurationService, IFileStorage fileStorage, HttpClient httpClient)
    {
        _configurationService = configurationService;
        _fileStorage = fileStorage;
        _httpClient = httpClient;
    }

    public async Task<bool> CheckForUpdatesAsync()
    {
        var gitHubOwner = (string)_configurationService.ReturnConfigValue(cfg => cfg.Common.GitHubOwner);
        var gitHubRepo = (string)_configurationService.ReturnConfigValue(cfg => cfg.Common.GitHubRepo);
        var includePrereleases = (bool)_configurationService.ReturnConfigValue(cfg => cfg.Common.IncludePrereleases);

        var currentVersion = GetCurrentVersion();

        try
        {
            var latestRelease = await GetLatestRelease(gitHubOwner, gitHubRepo, includePrereleases);
            if (latestRelease == null)
            {
                Log.Information("No releases found.");
                return false;
            }

            var latestVersion = latestRelease.TagName.TrimStart('v');
            if (IsNewerVersion(latestVersion, currentVersion))
            {
                Log.Information($"New version available: v{latestVersion}. Current version: v{currentVersion}");

                var asset = latestRelease.Assets.Find(a => a.Name.EndsWith(".zip"));
                if (asset != null)
                {
                    await DownloadAndInstallUpdate(asset.BrowserDownloadUrl);
                    return true;
                }

                Log.Warning("No suitable asset found in the release.");
            }
            else
            {
                Log.Information("Application is up to date.");
            }
            return false;
        }
        catch (Exception ex)
        {
            Log.Error($"Error checking for updates: {ex.Message}");
            return false;
        }
    }

    private async Task DownloadAndInstallUpdate(string downloadUrl)
    {
        string tempFilePath = Path.Combine(Path.GetTempPath(), "update.zip");
        Log.Information("Downloading update...");
        
        using var response = await _httpClient.GetAsync(downloadUrl);
        response.EnsureSuccessStatusCode();

        await using (var fs = _fileStorage.CreateFile(tempFilePath))
        {
            await response.Content.CopyToAsync(fs);
        }

        Log.Information("Download complete. Update is ready to be applied.");
        
    }

    private async Task<GitHubRelease> GetLatestRelease(string owner, string repo, bool includePrereleases)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PenumbraModForwarder");

        string url = $"https://api.github.com/repos/{owner}/{repo}/releases";
        if (!includePrereleases)
        {
            url += "/latest";
        }

        var responseString = await httpClient.GetStringAsync(url);

        if (includePrereleases)
        {
            var releases = JArray.Parse(responseString);
            foreach (var releaseToken in releases)
            {
                if (!releaseToken.Value<bool>("draft"))
                {
                    return ParseRelease(releaseToken);
                }
            }
        }
        else
        {
            var releaseToken = JObject.Parse(responseString);
            return ParseRelease(releaseToken);
        }

        return null;
    }
    
    private GitHubRelease ParseRelease(JToken releaseToken)
    {
        var assets = new List<GitHubAsset>();
        foreach (var assetToken in releaseToken["assets"])
        {
            assets.Add(new GitHubAsset
            {
                Name = assetToken.Value<string>("name"),
                BrowserDownloadUrl = assetToken.Value<string>("browser_download_url")
            });
        }

        return new GitHubRelease
        {
            TagName = releaseToken.Value<string>("tag_name"),
            Assets = assets
        };
    }
    
    private string GetCurrentVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return $"{version.Major}.{version.Minor}.{version.Build}";
    }

    private bool IsNewerVersion(string latestVersion, string currentVersion)
    {
        if (Version.TryParse(latestVersion, out var latest) && Version.TryParse(currentVersion, out var current))
        {
            return latest.CompareTo(current) > 0;
        }
        return false;
    }
    
    public class GitHubRelease
    {
        public string TagName { get; set; }
        public List<GitHubAsset> Assets { get; set; }
    }

    public class GitHubAsset
    {
        public string Name { get; set; }
        public string BrowserDownloadUrl { get; set; }
    }
}