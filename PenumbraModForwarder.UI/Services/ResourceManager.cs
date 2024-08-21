using System.Reflection;
using Microsoft.Extensions.Logging;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Interfaces;

namespace PenumbraModForwarder.UI.Services;

public class ResourceManager : IResourceManager
{
    private readonly ILogger<ResourceManager> _logger;
    private readonly IErrorWindowService _errorWindowService;
    
    public ResourceManager(ILogger<ResourceManager> logger, IErrorWindowService errorWindowService)
    {
        _logger = logger;
        _errorWindowService = errorWindowService;
    }

    public Icon LoadIcon(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null) return new Icon(stream);
        _logger.LogError($"Resource {resourceName} not found.");
        _errorWindowService.ShowError($"Resource {resourceName} not found.");
        throw new Exception($"Resource {resourceName} not found.");
    }

    public Image LoadImage(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null) return new Bitmap(stream);
        _logger.LogError($"Resource {resourceName} not found.");
        _errorWindowService.ShowError($"Resource {resourceName} not found.");
        throw new Exception($"Resource {resourceName} not found.");
    }
}