using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace BLivehimeNTR;

internal class BLiveWebSocketServer : BLiveServer<WebSocket>
{
    protected override (bool, ushort) GetServerConfig()
    {
        return (true, 19980);
    }

    internal async void StartAsync()
    {
        if (!Enable) return;
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{Port}/BLiveSMS/");
        try
        {
            listener.Start();
        }
        catch (Exception e)
        {
            InvokeServerStateChange($"WebSocket服务器在 ws://localhost:{Port}/BLiveSMS/ 上启动失败,原因:{e.Message}");
            return;
        }

        InvokeServerStateChange($"WebSocket服务器在 ws://localhost:{Port}/BLiveSMS/ 上启动成功");
        while (true)
        {
            var context = await listener.GetContextAsync();
            if (!context.Request.IsWebSocketRequest) continue;
            WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            var clientId = Guid.NewGuid();
            var clientSocket = webSocketContext.WebSocket;
            Clients.TryAdd(clientId, clientSocket);
            InvokeClientCountChange($"{Clients.Count}");
            HandleClient(clientId);
        }
    }

    protected override void ClientDispose(WebSocket clientSocket)
    {
        clientSocket.Dispose();
    }

    protected override Task ClientSendAsync(WebSocket clientSocket, byte[] rawData)
    {
        return clientSocket.SendAsync(rawData, WebSocketMessageType.Binary, true, CancellationToken.None);
    }
}