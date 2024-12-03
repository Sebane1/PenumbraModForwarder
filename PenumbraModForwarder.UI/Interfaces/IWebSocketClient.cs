using System.Threading.Tasks;

namespace PenumbraModForwarder.UI.Interfaces;

public interface IWebSocketClient
{
    Task ConnectAsync(int port);
}