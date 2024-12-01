using System.Threading.Tasks;

namespace PenumbraModForwarder.UI.Interfaces;

public interface IWebSocketClient
{
    public Task ConnectAsync();
}