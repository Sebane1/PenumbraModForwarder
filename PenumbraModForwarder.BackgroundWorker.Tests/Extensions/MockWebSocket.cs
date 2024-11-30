using System.Net.WebSockets;

namespace PenumbraModForwarder.BackgroundWorker.Tests.Extensions;

public class MockWebSocket : WebSocket
{
    private readonly TaskCompletionSource<bool> _receiveComplete = new();
    private bool _shouldClose;
    public List<byte[]> SentMessages { get; } = new();
    public bool CloseAsyncCalled { get; private set; }
    public override WebSocketState State => _shouldClose ? WebSocketState.Closed : WebSocketState.Open;
    public override string? SubProtocol { get; }
    public override WebSocketCloseStatus? CloseStatus { get; }
    public override string? CloseStatusDescription { get; }

    public void CompleteReceive(bool shouldClose = true)
    {
        _shouldClose = shouldClose;
        _receiveComplete.TrySetResult(true);
    }

    public override void Abort() { }

    public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
    {
        CloseAsyncCalled = true;
        return Task.CompletedTask;
    }

    public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override void Dispose() { }

    public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        var messageBytes = new byte[buffer.Count];
        Buffer.BlockCopy(buffer.Array!, buffer.Offset, messageBytes, 0, buffer.Count);
        SentMessages.Add(messageBytes);
        return Task.CompletedTask;
    }

    public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        await _receiveComplete.Task;
        return new WebSocketReceiveResult(0, _shouldClose ? WebSocketMessageType.Close : WebSocketMessageType.Text, true);
    }
}