using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BLivehimeNTR;

internal class BLiveTcpServer : BLiveServer<Socket>
{
    protected override (bool, ushort) GetServerConfig()
    {
        return (true, 19981);
    }

    internal async void StartAsync()
    {
        if (!Enable) return;
        var listener = new TcpListener(IPAddress.Any, Port);
        try
        {
            listener.Start();
        }
        catch (Exception e)
        {
            InvokeServerStateChange($"Tcp服务器在 localhost:{Port} 上启动失败,原因:{e.Message}");
            return;
        }

        InvokeServerStateChange($"Tcp服务器在 localhost:{Port} 上启动成功");
        while (true)
        {
            var clientSocket = await listener.AcceptTcpClientAsync();
            var clientId = Guid.NewGuid();
            Clients.TryAdd(clientId, clientSocket.Client);
            InvokeClientCountChange($"{Clients.Count}");
            HandleClient(clientId);
        }
    }

    protected override void ClientDispose(Socket clientSocket)
    {
        clientSocket.Dispose();
    }

    protected override Task<int> ClientSendAsync(Socket clientSocket, byte[] rawData)
    {
        return clientSocket.SendAsync(rawData, SocketFlags.None);
    }
}