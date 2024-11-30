using PenumbraModForwarder.BackgroundWorker.Extensions;
using PenumbraModForwarder.Common.Services;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplicationServices();

if (!ApplicationBootstrapper.IsInitializedByWatchdog())
{
    Log.Warning("Application must be started through the main executable");
    return;
}

var host = builder.Build();
host.Run();