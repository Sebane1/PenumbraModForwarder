using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Consts;
using Serilog;
using Serilog.Events;

namespace PenumbraModForwarder.Common.Extensions
{
    public static class Logging
    {
        public static void ConfigureLogging(IServiceCollection serviceCollection, string applicationName)
        {
            Directory.CreateDirectory(ConfigurationConsts.LogsPath);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}") // Update the output template
                .WriteTo.File(
                    path: Path.Combine(ConfigurationConsts.LogsPath, $"{applicationName}.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}" // Ensure the file output template includes SourceContext
                )
                .CreateLogger();

            serviceCollection.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });
        }
    }
}