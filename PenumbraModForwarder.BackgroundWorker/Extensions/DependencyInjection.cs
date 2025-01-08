﻿using PenumbraModForwarder.BackgroundWorker.Interfaces;
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
            
            services.AddTransient<IFileWatcher, FileWatcher>();
            services.AddSingleton<IFileQueueProcessor, FileQueueProcessor>();
            services.AddSingleton<IFileProcessor, FileProcessor>();
            
            services.AddHttpClient<IModInstallService, ModInstallService>(client =>
            {
                client.BaseAddress = new Uri(ApiConsts.BaseApiUrl);
            });
            return services;
        }

        public static void SetupLogging(this IServiceCollection services, string sentryDsn)
        {
            Logging.ConfigureLogging(services, "BackgroundWorker", sentryDsn);
        }

    }
}