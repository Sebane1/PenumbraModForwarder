using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Common.Interfaces;

public interface IStaticResourceService
{
    Task<(GithubStaticResources.InformationJson?, GithubStaticResources.UpdaterInformationJson?)>
        GetResourcesUsingGithubApiAsync();
}