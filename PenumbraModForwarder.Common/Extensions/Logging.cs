using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Consts;
using Serilog;
using Serilog.Events;

namespace PenumbraModForwarder.Common.Extensions;

public static class Logging
{
    // The core logger configuration pulling in the applicationName for distinct log files
    private static LoggerConfiguration GetBaseLoggerConfiguration(string applicationName)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(ConfigurationConsts.LogsPath, $"{applicationName}.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            );
    }

    public static void ConfigureLogging(IServiceCollection services, string applicationName)
    {
        Directory.CreateDirectory(ConfigurationConsts.LogsPath);
        Log.Logger = GetBaseLoggerConfiguration(applicationName).CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });
    }

    public static void EnableSentry(string sentryDns, string applicationName)
    {
        if (string.IsNullOrWhiteSpace(sentryDns))
        {
            Console.WriteLine("Sentry DSN not provided. Skipping Sentry enablement.");
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var semVersion = version == null
            ? "Local Build"
            : $"{version.Major}.{version.Minor}.{version.Build}";

        // Rebuild the logger to include Sentry, still respecting the applicationName-based file
        Log.Logger = GetBaseLoggerConfiguration(applicationName)
            .WriteTo.Sentry(sinkConfig =>
            {
                sinkConfig.Dsn = sentryDns;
                sinkConfig.MinimumEventLevel = LogEventLevel.Warning;
                sinkConfig.AttachStacktrace = true;
                sinkConfig.Release = semVersion;
                sinkConfig.AutoSessionTracking = true;

                // Set the sampling rates for performance tracing and profiling
                sinkConfig.TracesSampleRate = 1.0;
                sinkConfig.ProfilesSampleRate = 1.0;
            })
            .CreateLogger();

        Console.WriteLine("Sentry is now enabled at runtime.");
    }
}