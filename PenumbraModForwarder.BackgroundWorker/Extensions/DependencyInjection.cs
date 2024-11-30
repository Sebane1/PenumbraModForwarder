using PenumbraModForwarder.BackgroundWorker.Services;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Interfaces;

namespace PenumbraModForwarder.BackgroundWorker.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<IWebSocketServer, WebSocketServer>();
        services.SetupLogging();
        return services;
    }

    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "BackgroundWorker");
    }
}