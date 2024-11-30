using PenumbraModForwarder.Common.Extensions;

namespace PenumbraModForwarder.BackgroundWorker.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHostedService<Worker>();
        services.SetupLogging();
        return services;
    }

    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "BackgroundWorker");
    }
}