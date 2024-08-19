using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;
using Microsoft.Extensions.Logging;
using AutoMapper;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Services;
using Serilog;
using Serilog.Events;

namespace PenumbraModForwarder.UI.Extensions;

public static class ServiceExtensions
{
    public static IServiceProvider Configuration()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ConfigureViews(services);
        ConfigureLogging(services);
        ConfigureAutoMapper(services);

        return services.BuildServiceProvider();
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        // Registering services
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IFileWatcher, FileWatcher>();
        services.AddSingleton<IArchiveHelperService, ArchiveHelperService>();
        services.AddSingleton<IFileHandlerService, FileHandlerService>();
        services.AddSingleton<IFileSelector, FileSelector>();
        services.AddTransient<IPenumbraApi, PenumbraApi>();
        services.AddSingleton<IRegistryHelper, RegistryHelper>();
        services.AddSingleton<IPenumbraInstallerService, PenumbraInstallerService>();
    }
    
    private static void ConfigureViews(IServiceCollection services)
    {
        // Registering ViewModels and Views as transient
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
        services.AddTransient<FileSelectViewModel>();
        services.AddTransient<FileSelect>();
    }
    
    private static void ConfigureLogging(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, 
                restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();
        
        services.AddLogging(builder =>
        {
            builder.ClearProviders(); 
            builder.AddSerilog();
        });
    }

    private static void ConfigureAutoMapper(IServiceCollection services)
    {
        // AutoMapper configuration
        services.AddAutoMapper(typeof(MappingProfile)); 
    }
}