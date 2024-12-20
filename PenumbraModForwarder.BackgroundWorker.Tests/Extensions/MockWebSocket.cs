using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using PenumbraModForwarder.Common.Models;
using WebSocketMessageType = System.Net.WebSockets.WebSocketMessageType;

namespace PenumbraModForwarder.BackgroundWorker.Tests.Extensions;

public class MockWebSocket : WebSocket
{
    public List<byte[]> SentMessages { get; } = new List<byte[]>();
    public bool CloseAsyncCalled { get; private set; }

    private TaskCompletionSource<WebSocketReceiveResult> _receiveTcs = new();
    private ArraySegment<byte> _receiveBuffer;

    public override WebSocketCloseStatus? CloseStatus => WebSocketCloseStatus.NormalClosure;
    public override string CloseStatusDescription => "Closed";
    public override WebSocketState State => WebSocketState.Open;
    public override string SubProtocol => null;

    public override void Abort()
    {
        // No-op
    }

    public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
    {
        CloseAsyncCalled = true;
        return Task.CompletedTask;
    }

    public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        // No-op
    }

    public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        _receiveBuffer = buffer;
        return _receiveTcs.Task;
    }

    public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        SentMessages.Add(buffer.ToArray());
        return Task.CompletedTask;
    }

    /// <summary>
    /// Simulates receiving a message from the client.
    /// </summary>
    public void SimulateReceive(WebSocketMessage message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        Array.Copy(messageBytes, 0, _receiveBuffer.Array, _receiveBuffer.Offset, messageBytes.Length);
        _receiveTcs.SetResult(new WebSocketReceiveResult(messageBytes.Length, WebSocketMessageType.Text, true));
        _receiveTcs = new TaskCompletionSource<WebSocketReceiveResult>(); // Prepare for next message
    }

    /// <summary>
    /// Completes the receive loop to end the connection.
    /// </summary>
    public void CompleteReceive()
    {
        _receiveTcs.SetResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));
    }
}