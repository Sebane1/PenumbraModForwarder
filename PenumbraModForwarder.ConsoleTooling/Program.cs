using Microsoft.Extensions.DependencyInjection;
using PenumbraModForwarder.ConsoleTooling.Extensions;
using PenumbraModForwarder.ConsoleTooling.Interfaces;

var services = new ServiceCollection();
services.AddApplicationServices();

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