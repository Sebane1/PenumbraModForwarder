using PenumbraModForwarder.BackgroundWorker.Extensions;
using Serilog;
using ILogger = Serilog.ILogger;

bool isNewInstance;
using (new Mutex(true, "PenumbraModForwarder.BackgroundWorker", out isNewInstance))
{
    if (!isNewInstance)
    {
        Console.WriteLine("Another instance of PenumbraModForwarder.BackgroundWorker is already running. Exiting...");
        return;
    }

    var builder = Host.CreateApplicationBuilder(args);

    ILogger logger = Log.ForContext<Program>();

    bool isInitializedByWatchdog = Environment.GetEnvironmentVariable("WATCHDOG_INITIALIZED") == "true";

#if DEBUG
    // In debug mode, mark as initialized and assign a default port
    isInitializedByWatchdog = true;
    if (args.Length == 0)
    {
        args = new string[] { "12345" }; // Fixed port for debugging
    }
#endif

    logger.Information("Application initialized by watchdog: {IsInitialized}", isInitializedByWatchdog);

    if (!isInitializedByWatchdog)
    {
        logger.Warning("Application must be started through the main executable");
        return;
    }

    if (args.Length == 0)
    {
        logger.Fatal("No port specified for the BackgroundWorker.");
        return;
    }

    int port = int.Parse(args[0]);
    logger.Information("Starting BackgroundWorker on port {Port}", port);
    var services = new ServiceCollection();
    
    builder.Services.AddApplicationServices(port);

    var host = builder.Build();
    host.Run();
}