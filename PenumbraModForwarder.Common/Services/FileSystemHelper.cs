using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.Common.Services;

public class FileSystemHelper : IFileSystemHelper
{
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public IEnumerable<string> GetStandardTexToolsPaths()
    {
        var standardPaths = new List<string>();

        if (OperatingSystem.IsWindows())
        {
            // Add standard Windows installation paths
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            standardPaths.Add(Path.Combine(programFiles, "FFXIV TexTools", "FFXIV_TexTools", "ConsoleTools.exe"));
            standardPaths.Add(Path.Combine(programFilesX86, "FFXIV TexTools", "FFXIV_TexTools", "ConsoleTools.exe"));
        }
        //TODO: These need to be double checked
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // Add standard Unix-based installation paths
            standardPaths.Add("/usr/local/bin/FFXIV_TexTools/ConsoleTools");
            standardPaths.Add("/usr/bin/FFXIV_TexTools/ConsoleTools");
            // Add other common paths as needed
        }

        return standardPaths;
    }
}