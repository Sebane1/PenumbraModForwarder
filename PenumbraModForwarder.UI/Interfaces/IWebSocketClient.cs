using System.Threading.Tasks;
using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.UI.Interfaces;

public interface IWebSocketClient
{
    Task ConnectAsync(int port);
    Task SendMessageAsync(WebSocketMessage message, string endpoint);
}