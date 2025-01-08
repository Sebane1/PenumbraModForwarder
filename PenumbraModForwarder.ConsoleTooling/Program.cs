using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.ConsoleTooling.Extensions;
using PenumbraModForwarder.ConsoleTooling.Interfaces;

namespace PenumbraModForwarder.ConsoleTooling;

public class Program
{
    public static void Main(string[] args)
    {
        using var mutex = new Mutex(true, "PenumbraModForwarder.ConsoleTooling", out var isNewInstance);
        if (!isNewInstance)
        {
            Console.WriteLine("Another instance is already running. Exiting...");
            return;
        }
        
        var services = new ServiceCollection();
        services.AddApplicationServices();

        // Build the service provider
        using var serviceProvider = services.BuildServiceProvider();

        if (args.Length > 0)
        {
            var filePath = args[0];
            var installingService = serviceProvider.GetRequiredService<IInstallingService>();
            installingService.HandleFileAsync(filePath).GetAwaiter().GetResult();
        }
        else
        {
            Console.WriteLine("No file path was provided via the command line arguments.");
        }
    }
}