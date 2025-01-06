namespace PenumbraModForwarder.Common.Interfaces;

public interface IAria2Service
{
    string Aria2Folder { get; }
    string Aria2ExePath { get; }

    Task<bool> EnsureAria2AvailableAsync();
    Task<bool> DownloadFileAsync(string fileUrl, string downloadDirectory);
}
