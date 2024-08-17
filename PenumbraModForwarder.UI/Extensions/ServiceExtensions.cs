using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;
using Microsoft.Extensions.Logging;
using AutoMapper;
using PenumbraModForwarder.Common.Models;

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
        // Setting up logging to console
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
    }

    private static void ConfigureAutoMapper(IServiceCollection services)
    {
        // AutoMapper configuration
        services.AddAutoMapper(typeof(MappingProfile)); 
    }
}