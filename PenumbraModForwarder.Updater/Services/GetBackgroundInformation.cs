using System.Threading.Tasks;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.Common.Models;
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

    public async Task<(GithubStaticResources.InformationJson?, GithubStaticResources.UpdaterInformationJson?)> GetResources()
    {
        var resources = await _staticResourceService.GetResourcesUsingGithubApiAsync();
        return resources;
    }
}