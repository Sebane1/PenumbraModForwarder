using PenumbraModForwarder.Common.Models;

namespace PenumbraModForwarder.BackgroundWorker.Events;


public class WebSocketMessageEventArgs : EventArgs
{
    public string Endpoint { get; set; }
    public WebSocketMessage Message { get; set; }
}