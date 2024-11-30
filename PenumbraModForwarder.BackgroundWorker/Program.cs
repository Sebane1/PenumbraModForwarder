using PenumbraModForwarder.BackgroundWorker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplicationServices();

var host = builder.Build();
host.Run();