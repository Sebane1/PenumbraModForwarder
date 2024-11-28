using PenumbraModForwarder.Watchdog.Services;

namespace PenumbraModForwarder.Watchdog;

class Program
{
    static void Main(string[] args)
    {
        var processManager = new ProcessManager();
        
        processManager.Run();
    }
}