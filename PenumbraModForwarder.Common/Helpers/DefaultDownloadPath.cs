using System.Runtime.InteropServices;

namespace PenumbraModForwarder.Common.Helpers;

public static class DefaultDownloadPath
{
    public static string GetDefaultDownloadPath()
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var downloadPath = string.Empty;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            downloadPath = Path.Combine(homePath, @"Downloads\");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            downloadPath = Path.Combine(homePath, @"Downloads\");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            downloadPath = Path.Combine(homePath, @"Downloads\");
        }
        else
        {
            downloadPath = homePath;
        }

        return downloadPath;
    }
}