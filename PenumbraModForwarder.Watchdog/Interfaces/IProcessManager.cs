using System.Diagnostics;

namespace PenumbraModForwarder.Watchdog.Interfaces;

public interface IProcessManager : IDisposable
{
    public void Run();
    public void Dispose();
}