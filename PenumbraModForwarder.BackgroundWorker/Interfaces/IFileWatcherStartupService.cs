namespace PenumbraModForwarder.BackgroundWorker.Interfaces;

public interface IFileWatcherService : IDisposable
{
    Task Start();
    void Stop();
}