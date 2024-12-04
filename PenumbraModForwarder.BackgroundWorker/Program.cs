using PenumbraModForwarder.BackgroundWorker.Extensions;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

bool isInitializedByWatchdog = Environment.GetEnvironmentVariable("WATCHDOG_INITIALIZED") == "true";

#if DEBUG
isInitializedByWatchdog = true;
if (args.Length == 0)
{
    args = new string[] { "12345" }; // Fixed port for debugging
}
#endif

Log.Information($"Application initialized by watchdog: {isInitializedByWatchdog}");

if (!isInitializedByWatchdog)
{
    Log.Warning("Application must be started through the main executable");
    return;
}

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