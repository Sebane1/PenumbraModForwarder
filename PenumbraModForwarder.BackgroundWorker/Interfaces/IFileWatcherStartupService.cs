namespace PenumbraModForwarder.BackgroundWorker.Interfaces;

public interface IFileWatcherStartupService : IDisposable
{
    void Start();
    void Stop();
}