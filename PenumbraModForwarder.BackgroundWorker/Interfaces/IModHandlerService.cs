namespace PenumbraModForwarder.BackgroundWorker.Interfaces;

public interface IModHandlerService
{
    Task HandleFileAsync(string filePath);
}