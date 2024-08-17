﻿using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Services;
using PenumbraModForwarder.UI.ViewModels;
using PenumbraModForwarder.UI.Views;
using Microsoft.Extensions.Logging;

namespace PenumbraModForwarder.UI.Extensions;

public static class ServiceExtensions
{
    public static ServiceProvider Configuration()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ConfigureViews(services);
        ConfigureLogging(services);

        return services.BuildServiceProvider();
    }
    
    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<IConfigurationService, ConfigurationService>();
    }
    
    private static void ConfigureViews(ServiceCollection services)
    {
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
    }
    
    private static void ConfigureLogging(ServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
    }
}