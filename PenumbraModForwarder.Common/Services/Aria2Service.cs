using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;
using PenumbraModForwarder.Common.Interfaces;
using Serilog;

namespace PenumbraModForwarder.Common.Services;

public class Aria2Service : IAria2Service
{
    private readonly ILogger _logger;
    private const string Aria2LatestReleaseApi = "https://api.github.com/repos/aria2/aria2/releases/latest";

    public string Aria2Folder { get; }
    public string Aria2ExePath => Path.Combine(Aria2Folder, "aria2c.exe");

    public Aria2Service(string aria2InstallFolder)
    {
        _logger = Log.ForContext<Aria2Service>();
        Aria2Folder = aria2InstallFolder;
        _ = EnsureAria2AvailableAsync();
    }

    public async Task<bool> EnsureAria2AvailableAsync()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!File.Exists(Aria2ExePath))
            {
                _logger.Information("aria2 not found at '{Aria2Exe}'. Checking the latest release on GitHub...", Aria2ExePath);
                return await DownloadAndInstallAria2FromLatestAsync();
            }

            _logger.Information("aria2 located: {Aria2Exe}", Aria2ExePath);
            return true;
        }
        else
        {
            _logger.Warning("Service is only set up for Windows platforms in this implementation.");
            return false;
        }
    }
    
    public async Task<bool> DownloadFileAsync(string fileUrl, string downloadDirectory)
    {
        var isReady = await EnsureAria2AvailableAsync();
        if (!isReady)
            return false;

        try
        {
            // Remove any trailing slashes
            var sanitizedDirectory = downloadDirectory.TrimEnd('\\', '/');

            // Derive the file name (including extension) from the remote URL
            var rawFileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
            if (string.IsNullOrWhiteSpace(rawFileName))
            {
                rawFileName = "download.bin";
            }

            // Decode percent-encoded characters (e.g. "%20" -> space, "%27" -> apostrophe)
            var finalFileName = Uri.UnescapeDataString(rawFileName);

            var extraAria2Args = "--log-level=debug";
            var arguments = $"\"{fileUrl}\" --dir=\"{sanitizedDirectory}\" --out=\"{finalFileName}\" {extraAria2Args}";

            var startInfo = new ProcessStartInfo
            {
                FileName = Aria2ExePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _logger.Debug("Launching aria2 with arguments: {Args}", startInfo.Arguments);

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.Error("Failed to start aria2 at {ExePath}", Aria2ExePath);
                return false;
            }

            var stdOutTask = process.StandardOutput.ReadToEndAsync();
            var stdErrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var stdOut = await stdOutTask;
            var stdErr = await stdErrTask;

            if (!string.IsNullOrWhiteSpace(stdOut))
            {
                _logger.Debug("[aria2 STDOUT] {Output}", stdOut.TrimEnd());
            }
            if (!string.IsNullOrWhiteSpace(stdErr))
            {
                _logger.Debug("[aria2 STDERR] {Output}", stdErr.TrimEnd());
            }

            if (process.ExitCode == 0)
            {
                _logger.Information(
                    "aria2 finished downloading '{FileUrl}' to '{Folder}\\{OutFile}'",
                    fileUrl,
                    sanitizedDirectory,
                    finalFileName
                );
                return true;
            }
            else
            {
                _logger.Error("aria2 exited with code {ExitCode} for {FileUrl}", process.ExitCode, fileUrl);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error downloading '{FileUrl}' via aria2.", fileUrl);
            return false;
        }
    }

    private async Task<bool> DownloadAndInstallAria2FromLatestAsync()
    {
        try
        {
            if (!Directory.Exists(Aria2Folder))
            {
                Directory.CreateDirectory(Aria2Folder);
            }

            var downloadUrl = await FetchWin64AssetUrlAsync();
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                _logger.Error("No matching Windows 64-bit asset URL found. Cannot install aria2.");
                return false;
            }

            var zipPath = Path.Combine(Aria2Folder, "aria2_latest_win64.zip");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "PenumbraModForwarder/Aria2Service");
                _logger.Information("Downloading aria2 from {Url}", downloadUrl);

                var bytes = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(zipPath, bytes);
            }

            _logger.Information("Extracting aria2 files to: {Folder}", Aria2Folder);
            ZipFile.ExtractToDirectory(zipPath, Aria2Folder, overwriteFiles: true);

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            if (!File.Exists(Aria2ExePath))
            {
                var exeCandidate = Directory
                    .GetFiles(Aria2Folder, "aria2c.exe", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (exeCandidate != null)
                {
                    var targetPath = Path.Combine(Aria2Folder, Path.GetFileName(exeCandidate));
                    if (!File.Exists(targetPath))
                    {
                        File.Move(exeCandidate, targetPath);
                        _logger.Information("aria2c.exe found in a subdirectory and moved to: {TargetPath}", targetPath);
                    }
                }
            }

            if (!File.Exists(Aria2ExePath))
            {
                _logger.Error("Setup failed. '{Aria2ExePath}' not found after extraction.", Aria2ExePath);
                return false;
            }

            _logger.Information("Successfully installed aria2 at {Aria2ExePath}", Aria2ExePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching/installing aria2.");
            return false;
        }
    }

    private async Task<string?> FetchWin64AssetUrlAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "PenumbraModForwarder/Aria2Service");

            var json = await client.GetStringAsync(Aria2LatestReleaseApi);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("assets", out var assetsElement) ||
                assetsElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var asset in assetsElement.EnumerateArray())
            {
                if (!asset.TryGetProperty("name", out var assetNameElement) ||
                    !asset.TryGetProperty("browser_download_url", out var downloadUrlElement))
                {
                    continue;
                }

                var assetName = assetNameElement.GetString() ?? string.Empty;
                var downloadUrl = downloadUrlElement.GetString() ?? string.Empty;

                if (assetName.Contains("win-64", StringComparison.OrdinalIgnoreCase) &&
                    assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return downloadUrl;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch the latest aria2 release data from GitHub.");
        }

        return null;
    }
}