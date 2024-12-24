using System;
using System.Threading.Tasks;
using PenumbraModForwarder.Common.Models;
using PenumbraModForwarder.UI.Events;

namespace PenumbraModForwarder.UI.Interfaces;

public interface IWebSocketClient
{
    Task ConnectAsync(int port);
    Task SendMessageAsync(WebSocketMessage message, string endpoint);
    event EventHandler<FileSelectionRequestedEventArgs> FileSelectionRequested;
}