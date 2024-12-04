using PenumbraModForwarder.BackgroundWorker.Extensions;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Check if the application is initialized by the Watchdog
bool isInitializedByWatchdog = Environment.GetEnvironmentVariable("WATCHDOG_INITIALIZED") == "true";

#if DEBUG
isInitializedByWatchdog = true;
#endif

Log.Information($"Application initialized by watchdog: {isInitializedByWatchdog}");

if (!isInitializedByWatchdog)
{
    Log.Warning("Application must be started through the main executable");
    return;
}

#if DEBUG
// In debug mode, provide a default port if none is specified
if (args.Length == 0)
{
    args = ["12345"];
}
#endif

if (args.Length == 0)
{
    Log.Fatal("No port specified for the BackgroundWorker.");
    return;
}

int port = int.Parse(args[0]);
Log.Information($"Starting BackgroundWorker on port {port}");

builder.Services.AddApplicationServices(port);

var host = builder.Build();
host.Run();