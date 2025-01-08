using PenumbraModForwarder.BackgroundWorker.Interfaces;
using PenumbraModForwarder.BackgroundWorker.Services;
using PenumbraModForwarder.Common.Consts;
using PenumbraModForwarder.Common.Extensions;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.FileMonitor.Interfaces;
using PenumbraModForwarder.FileMonitor.Services;
using PenumbraModForwarder.Statistics.Services;

namespace PenumbraModForwarder.BackgroundWorker.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, int port)
        {
            services.SetupLogging();
            
            services.AddHostedService(provider => new Worker(
                provider.GetRequiredService<IWebSocketServer>(),
                provider.GetRequiredService<IStartupService>(),
                port,
                provider.GetRequiredService<IHostApplicationLifetime>()
            ));
            
            services.AddSingleton<IWebSocketServer, WebSocketServer>();
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<IFileWatcherService, FileWatcherService>();
            services.AddSingleton<ITexToolsHelper, TexToolsHelper>();
            services.AddSingleton<IRegistryHelper, RegistryHelper>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IFileStorage, FileStorage>();
            services.AddSingleton<IFileSystemHelper, FileSystemHelper>();
            services.AddSingleton<IModHandlerService, ModHandlerService>();
            services.AddSingleton<IStatisticService, StatisticService>();
            services.AddSingleton<IPenumbraService, PenumbraService>();
            services.AddSingleton<IConfigurationListener, ConfigurationListener>();
            services.AddTransient<IFileWatcher, FileWatcher>();
            services.AddSingleton<IFileQueueProcessor, FileQueueProcessor>();
            services.AddSingleton<IFileProcessor, FileProcessor>();
            
            services.AddHttpClient<IModInstallService, ModInstallService>(client =>
            {
                client.BaseAddress = new Uri(ApiConsts.BaseApiUrl);
            });
            
            return services;
        }
        
        public static void EnableSentryLogging()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();

            var sentryDns = configuration["SENTRY_DNS"];
            if (string.IsNullOrWhiteSpace(sentryDns))
            {
                Console.WriteLine("No SENTRY_DSN provided. Skipping Sentry enablement.");
                return;
            }

            Logging.EnableSentry(sentryDns, "BackgroundWorker");
        }
        
        public static void DisableSentryLogging()
        {
            Logging.DisableSentry("BackgroundWorker");
        }

        private static void SetupLogging(this IServiceCollection services)
        {
            Logging.ConfigureLogging(services, "BackgroundWorker");
        }
    }
}