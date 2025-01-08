using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Consts;
using Serilog;
using Serilog.Events;

namespace PenumbraModForwarder.Common.Extensions
{
    public static class Logging
    {
        private static readonly LoggerConfiguration BaseLoggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(ConfigurationConsts.LogsPath, "Application.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            );

        public static void ConfigureLogging(IServiceCollection services, string applicationName)
        {
            // Ensure the log directory exists
            Directory.CreateDirectory(ConfigurationConsts.LogsPath);

            // Create a logger without Sentry initially
            Log.Logger = BaseLoggerConfiguration.CreateLogger();

            // Attach to the application logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });
        }

        public static void EnableSentry(string sentryDsn)
        {
            if (string.IsNullOrWhiteSpace(sentryDsn))
            {
                Console.WriteLine("Sentry DSN not provided. Skipping Sentry enablement.");
                return;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var semVersion = version == null
                ? "Local Build"
                : $"{version.Major}.{version.Minor}.{version.Build}";

            // Rebuild logger to include Sentry
            Log.Logger = BaseLoggerConfiguration
                .WriteTo.Sentry(sinkConfig =>
                {
                    sinkConfig.Dsn = sentryDsn;
                    sinkConfig.MinimumEventLevel = LogEventLevel.Warning;
                    sinkConfig.AttachStacktrace = true;
                    sinkConfig.Release = semVersion;
                    sinkConfig.AutoSessionTracking = true;
                })
                .CreateLogger();

            Console.WriteLine("Sentry is now enabled at runtime.");
        }
    }
}