using PenumbraModForwarder.Watchdog.Services;

namespace PenumbraModForwarder.Watchdog;

class Program
{
    static void Main(string[] args)
    {
        bool isDevMode = Environment.GetEnvironmentVariable("DEV_MODE") == "true";
        
        var processManager = new ProcessManager(isDevMode);

        var uiProcess = processManager.StartProcess("PenumbraModForwarder.UI");
        var backgroundServiceProcess = processManager.StartProcess("PenumbraModForwarder.BackgroundWorker");

        if (uiProcess == null || backgroundServiceProcess == null)
        {
            Console.WriteLine("Failed to start one or more processes. Exiting.");
            Environment.Exit(1);
        }

        processManager.MonitorProcesses(uiProcess, backgroundServiceProcess);
    }
}