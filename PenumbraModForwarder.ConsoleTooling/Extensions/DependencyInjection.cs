using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.ConsoleTooling.Interfaces;
using PenumbraModForwarder.ConsoleTooling.Services;
using PenumbraModForwarder.Statistics.Services;

namespace PenumbraModForwarder.ConsoleTooling.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.SetupLogging();
        services.AddSingleton<ISoundManagerService, SoundManagerService>();
        services.AddSingleton<IInstallingService, InstallingService>();
        services.AddSingleton<IPenumbraService, PenumbraService>();
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IStatisticService, StatisticService>();
        services.AddSingleton<IPenumbraService, PenumbraService>();

        services.AddHttpClient<IModInstallService, ModInstallService>(client =>
        {
            client.BaseAddress = new Uri(ApiConsts.BaseApiUrl);
        });

        return services;
    }
    
    private static void SetupLogging(this IServiceCollection services)
    {
        Logging.ConfigureLogging(services, "ConsoleTool");
    }
    
    public static void EnableSentryLogging()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        var sentryDsn = configuration["SENTRY_DSN"];
        if (string.IsNullOrWhiteSpace(sentryDsn))
        {
            Console.WriteLine("No SENTRY_DSN provided. Skipping Sentry enablement.");
            return;
        }

        Logging.EnableSentry(sentryDsn);
    }
}