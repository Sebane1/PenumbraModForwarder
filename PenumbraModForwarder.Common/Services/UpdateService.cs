using System.Net.Http.Headers;
using PenumbraModForwarder.Common.Extensions;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.Common.Services;

public class UpdateService : IUpdateService
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger _logger;

    public UpdateService(IConfigurationService configurationService)
    {
        _logger = Log.Logger.ForContext<UpdateService>();
        _configurationService = configurationService;
    }

    private class GitHubRelease
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("prerelease")]
        public bool Prerelease { get; set; }

        [JsonProperty("assets")]
        public List<GitHubAsset> Assets { get; set; }
    }

    private class GitHubAsset
    {
        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public async Task<List<string>> GetUpdateZipLinksAsync(string currentVersion)
    {
        _logger.Debug("Entered GetUpdateZipLinksAsync. CurrentVersion: {CurrentVersion}", currentVersion);

        var includePrerelease = (bool)_configurationService.ReturnConfigValue(c => c.Common.IncludePrereleases);
        _logger.Debug("IncludePrerelease: {IncludePrerelease}", includePrerelease);

        var latestRelease = await GetLatestReleaseAsync(includePrerelease);
        if (latestRelease == null)
        {
            _logger.Debug("No GitHub releases found. Returning an empty list.");
            return new List<string>();
        }

        _logger.Debug("Latest release found: {TagName}. Checking if it is newer than currentVersion.", latestRelease.TagName);

        if (IsVersionGreater(latestRelease.TagName, currentVersion))
        {
            _logger.Debug("A newer version is available: {TagName}. Current: {CurrentVersion}",
                          latestRelease.TagName, currentVersion);

            var zipLinks = latestRelease.Assets
                .Where(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.BrowserDownloadUrl)
                .ToList();

            _logger.Debug("Found {Count} .zip asset(s) in the latest release.", zipLinks.Count);
            return zipLinks;
        }

        _logger.Debug("Current version is up-to-date. Returning an empty list.");
        return new List<string>();
    }
    
    public async Task<bool> NeedsUpdateAsync(string currentVersion)
    {
        _logger.Debug("Entered NeedsUpdateAsync. CurrentVersion: {CurrentVersion}", currentVersion);

        var includePrerelease = (bool)_configurationService.ReturnConfigValue(c => c.Common.IncludePrereleases);
        _logger.Debug("IncludePrerelease: {IncludePrerelease}", includePrerelease);

        var latestRelease = await GetLatestReleaseAsync(includePrerelease);
        if (latestRelease == null)
        {
            _logger.Debug("No GitHub releases returned. No update needed.");
            return false;
        }

        var result = IsVersionGreater(latestRelease.TagName, currentVersion);
        _logger.Debug("IsVersionGreater returned {Result} for latest release {TagName}", result, latestRelease.TagName);

        return result;
    }

    public async Task<string> GetMostRecentVersionAsync()
    {
        _logger.Debug("Entered GetMostRecentVersionAsync.");

        var includePrerelease = (bool)_configurationService.ReturnConfigValue(c => c.Common.IncludePrereleases);
        _logger.Debug("IncludePrerelease: {IncludePrerelease}", includePrerelease);

        var latestRelease = await GetLatestReleaseAsync(includePrerelease);
        if (latestRelease == null)
        {
            _logger.Debug("No releases found. Returning empty string.");
            return string.Empty;
        }

        _logger.Debug("Latest release version found: {TagName}", latestRelease.TagName);

        // If it's a prerelease, return the version number with "-b" appended
        if (latestRelease.Prerelease)
        {
            _logger.Debug("Release is a prerelease. Returning {TagName}-b.", latestRelease.TagName);
            return $"{latestRelease.TagName}-b";
        }

        return latestRelease.TagName;
    }

    private async Task<GitHubRelease?> GetLatestReleaseAsync(bool includePrerelease)
    {
        _logger.Debug("Entered GetLatestReleaseAsync. includePrerelease: {IncludePrerelease}", includePrerelease);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("PenumbraModForwarder", "1.0"));

        const string url = "https://api.github.com/repos/Sebane1/PenumbraModForwarder/releases";
        using var response = await httpClient.GetAsync(url);

        _logger.Debug("GitHub releases GET request completed with status code {StatusCode}", response.StatusCode);

        var releases = await response.Content.ReadAsJsonAsync<List<GitHubRelease>>();
        if (releases == null || releases.Count == 0)
        {
            _logger.Debug("No releases were deserialized or the list is empty.");
            return null;
        }

        _logger.Debug("Found {Count} releases. Filter prerelease: {FilterPrerelease}", releases.Count, !includePrerelease);

        var filtered = includePrerelease
            ? releases
            : releases.Where(r => !r.Prerelease).ToList();

        var latestRelease = filtered.FirstOrDefault();
        if (latestRelease == null)
        {
            _logger.Debug("No suitable release found after filtering prereleases.");
        }
        else
        {
            _logger.Debug("Using release with tag_name {TagName}", latestRelease.TagName);
        }

        return latestRelease;
    }
    
    private bool IsVersionGreater(string newVersion, string oldVersion)
    {
        _logger.Debug("IsVersionGreater check. New: {NewVersion}, Old: {OldVersion}", newVersion, oldVersion);

        if (string.IsNullOrWhiteSpace(newVersion) || string.IsNullOrWhiteSpace(oldVersion))
        {
            _logger.Debug("One or both version strings were null/empty. Returning false.");
            return false;
        }

        var splittedNew = newVersion.Split('.');
        var splittedOld = oldVersion.Split('.');

        if (splittedNew.Length != 3 || splittedOld.Length != 3)
        {
            _logger.Debug("Version not in x.x.x format. Reverting to ordinal compare.");
            return string.CompareOrdinal(newVersion, oldVersion) > 0;
        }

        if (!int.TryParse(splittedNew[0], out var majorNew) ||
            !int.TryParse(splittedNew[1], out var minorNew) ||
            !int.TryParse(splittedNew[2], out var patchNew))
        {
            _logger.Debug("Error parsing newVersion to integers. Using ordinal compare.");
            return string.CompareOrdinal(newVersion, oldVersion) > 0;
        }

        if (!int.TryParse(splittedOld[0], out var majorOld) ||
            !int.TryParse(splittedOld[1], out var minorOld) ||
            !int.TryParse(splittedOld[2], out var patchOld))
        {
            _logger.Debug("Error parsing oldVersion to integers. Using ordinal compare.");
            return string.CompareOrdinal(newVersion, oldVersion) > 0;
        }

        if (majorNew > majorOld) return true;
        if (majorNew < majorOld) return false;

        if (minorNew > minorOld) return true;
        if (minorNew < minorOld) return false;

        return patchNew > patchOld;
    }
}