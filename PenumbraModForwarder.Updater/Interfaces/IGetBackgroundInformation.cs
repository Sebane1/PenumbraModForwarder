using System.Threading.Tasks;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.Updater.Interfaces;

public interface IGetBackgroundInformation
{
    Task<(GithubStaticResources.InformationJson?, GithubStaticResources.UpdaterInformationJson?)> GetResources();
}