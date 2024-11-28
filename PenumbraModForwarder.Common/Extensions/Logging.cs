using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Consts;
using Serilog;

namespace PenumbraModForwarder.Common.Extensions;

public static class Logging
{
    public static void ConfigureLogging(IServiceCollection serviceCollection, string applicationName)
    {
        Directory.CreateDirectory(ConfigurationConsts.LogsPath);
        
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(ConfigurationConsts.LogsPath + $"\\{applicationName}.log", 
                rollingInterval: RollingInterval.Day, 
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024,  
                rollOnFileSizeLimit: true )
            .CreateLogger();
        
        serviceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });
    }
}