using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using NLog;
using PenumbraModForwarder.Common.Interfaces;
using SevenZipExtractor;

namespace PenumbraModForwarder.Common.Services;

public class Aria2Service : IAria2Service
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private bool _aria2Ready;
    private const string Aria2LatestReleaseApi = "https://api.github.com/repos/aria2/aria2/releases/latest";

    public string Aria2Folder { get; }
    public string Aria2ExePath => Path.Combine(Aria2Folder, "aria2c.exe");

    public Aria2Service(string aria2InstallFolder)
    {
        Aria2Folder = aria2InstallFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // Begin ensuring aria2 is installed asynchronously
        _ = EnsureAria2AvailableAsync(CancellationToken.None);
    }

    public async Task<bool> EnsureAria2AvailableAsync(CancellationToken ct)
    {
        if (_aria2Ready) 
            return true;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _logger.Warn("Service is only supported on Windows platforms in this implementation.");
            return false;
        }

        if (!File.Exists(Aria2ExePath))
        {
            _logger.Info("aria2 not found at '{Path}'. Checking the latest release on GitHub...", Aria2ExePath);
            var installed = await DownloadAndInstallAria2FromLatestAsync(ct);
            _aria2Ready = installed;
            return installed;
        }

        _logger.Info("aria2 located at {Path}", Aria2ExePath);
        _aria2Ready = true;
        return true;
    }

    public async Task<bool> DownloadFileAsync(string fileUrl, string downloadDirectory, CancellationToken ct)
    {
        var isReady = await EnsureAria2AvailableAsync(ct);
        if (!isReady) 
            return false;

        try
        {
            var sanitizedDirectory = downloadDirectory.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            );

            var rawFileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
            if (string.IsNullOrWhiteSpace(rawFileName))
                rawFileName = "download.bin";

            var finalFileName = Uri.UnescapeDataString(rawFileName);

            // Add extra arguments if desired
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
                _logger.Error("Failed to start aria2 at {Aria2ExePath}", Aria2ExePath);
                return false;
            }

            var stdOutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stdErrTask = process.StandardError.ReadToEndAsync(ct);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromMinutes(30));

            await process.WaitForExitAsync(timeoutCts.Token);

            var stdOut = await stdOutTask;
            var stdErr = await stdErrTask;

            if (!string.IsNullOrWhiteSpace(stdOut))
            {
                _logger.Debug("[aria2 STDOUT] {Output}", stdOut.Trim());
            }
            if (!string.IsNullOrWhiteSpace(stdErr))
            {
                _logger.Debug("[aria2 STDERR] {Output}", stdErr.Trim());
            }

            if (process.ExitCode == 0)
            {
                _logger.Info(
                    "aria2 finished downloading {FileUrl} to {Directory}\\{FileName}",
                    fileUrl,
                    sanitizedDirectory,
                    finalFileName
                );
                return true;
            }

            _logger.Error("aria2 exited with code {Code} for {Url}", process.ExitCode, fileUrl);
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger.Warn("Download canceled for {FileUrl}", fileUrl);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error downloading {FileUrl} via aria2", fileUrl);
            return false;
        }
    }

    private async Task<bool> DownloadAndInstallAria2FromLatestAsync(CancellationToken ct)
    {
        try
        {
            if (!Directory.Exists(Aria2Folder))
            {
                Directory.CreateDirectory(Aria2Folder);
            }

            var downloadUrl = await FetchWin64AssetUrlAsync(ct);
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                _logger.Error("No matching Windows 64-bit asset URL found. Cannot install aria2.");
                return false;
            }

            var zipPath = Path.Combine(Aria2Folder, "aria2_latest_win64.zip");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "PenumbraModForwarder/Aria2Service");
                var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();

                _logger.Info("Downloading aria2 from {Url}", downloadUrl);

                await using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs, ct);
            }

            _logger.Info("Extracting aria2 files to {ExtractPath}", Aria2Folder);

            using (var archiveFile = new ArchiveFile(zipPath))
            {
                archiveFile.Extract(Aria2Folder, overwrite: true);
            }

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

                        _logger.Info("Found aria2c.exe in a subdirectory; moved it to {TargetPath}", targetPath);

                        var candidateFolder = Path.GetDirectoryName(exeCandidate);
                        if (!string.IsNullOrEmpty(candidateFolder) &&
                            !candidateFolder.Equals(Aria2Folder, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                _logger.Info("Removing extracted folder {Folder}", candidateFolder);
                                Directory.Delete(candidateFolder, recursive: true);
                                _logger.Info("Successfully removed folder {Folder}", candidateFolder);
                            }
                            catch (Exception ex)
                            {
                                _logger.Warn(ex, "Failed to remove folder {Folder}", candidateFolder);
                            }
                        }
                    }
                }
            }

            if (!File.Exists(Aria2ExePath))
            {
                _logger.Error("Setup failed. {Aria2ExePath} not found after extraction.", Aria2ExePath);
                return false;
            }

            _logger.Info("Successfully installed aria2 at {Aria2ExePath}", Aria2ExePath);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.Warn("Download or setup was canceled.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching/installing aria2.");
            return false;
        }
    }

    private async Task<string?> FetchWin64AssetUrlAsync(CancellationToken ct)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "PenumbraModForwarder/Aria2Service");

            var json = await client.GetStringAsync(Aria2LatestReleaseApi, ct);
            var doc = JToken.Parse(json);

            if (doc["assets"] is not JArray assetsArray)
            {
                return null;
            }

            foreach (var asset in assetsArray)
            {
                var assetName = (string?)asset["name"] ?? string.Empty;
                var downloadUrl = (string?)asset["browser_download_url"] ?? string.Empty;

                if (assetName.Contains("win-64", StringComparison.OrdinalIgnoreCase)
                    && assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return downloadUrl;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Warn("Fetching Windows 64-bit asset URL was canceled.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch the latest aria2 release data from GitHub.");
        }

        return null;
    }
}