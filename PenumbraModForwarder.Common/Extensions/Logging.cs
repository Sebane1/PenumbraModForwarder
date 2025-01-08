using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Consts;
using Serilog;
using Serilog.Events;

namespace PenumbraModForwarder.Common.Extensions;

public static class Logging
{
    public static void ConfigureLogging(IServiceCollection serviceCollection, string applicationName, string sentryDsn)
    {
        #region version

        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var semVersion = version == null
            ? "Local Build"
            : $"{version.Major}.{version.Minor}.{version.Build}";

        #endregion

        Directory.CreateDirectory(ConfigurationConsts.LogsPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(ConfigurationConsts.LogsPath, $"{applicationName}.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.Sentry(sinkConfig =>
            {
                if (string.IsNullOrWhiteSpace(sentryDsn))
                {
                    Console.WriteLine("SENTRY_DSN is not provided. Sentry logging is disabled.");
                    return;
                }

                sinkConfig.Dsn = sentryDsn;
                sinkConfig.MinimumEventLevel = LogEventLevel.Warning;
                sinkConfig.AttachStacktrace = true;
                sinkConfig.Release = semVersion;
                sinkConfig.AutoSessionTracking = true;
            })
            .CreateLogger();

        serviceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });
    }

}