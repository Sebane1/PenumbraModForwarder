using System.Threading.Tasks;

namespace PenumbraModForwarder.Updater.Interfaces;

public interface IDownloadAndInstallUpdates
{
    Task<(bool success, string downloadPath)> DownloadAndInstallAsync(string currentVersion);
}