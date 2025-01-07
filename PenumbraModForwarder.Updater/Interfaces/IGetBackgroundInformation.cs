using System.Threading.Tasks;

namespace PenumbraModForwarder.Updater.Interfaces;

public interface IGetBackgroundInformation
{
    Task GetResources();
}