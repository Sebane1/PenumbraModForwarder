using System.Threading.Tasks;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Updater.Interfaces;
using Serilog;

namespace PenumbraModForwarder.Updater.Services;

public class GetBackgroundInformation : IGetBackgroundInformation
{
    private readonly ILogger _logger;
    private readonly IStaticResourceService _staticResourceService;

    public GetBackgroundInformation(IStaticResourceService staticResourceService)
    {
        _staticResourceService = staticResourceService;
        _logger = Log.ForContext<GetBackgroundInformation>();
    }

    public async Task GetResources()
    {
        var resources = await _staticResourceService.GetResourcesUsingGithubApiAsync();
        var mainInfo = resources.Item1;
        var updaterInfo = resources.Item2;
        
        _logger.Debug($"Main info: {mainInfo.Name}");
        _logger.Debug($"Updater info: {updaterInfo.Name}");
    }
}