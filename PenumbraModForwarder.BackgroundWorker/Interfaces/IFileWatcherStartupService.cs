namespace PenumbraModForwarder.BackgroundWorker.Interfaces;

public interface IFileWatcherService : IDisposable
{
    void Start();
    void Stop();
}