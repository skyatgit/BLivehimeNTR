using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BLivehimeNTR;

internal abstract class BLiveServer<T>
{
    protected readonly ConcurrentDictionary<Guid, T> Clients = new();
    protected readonly bool Enable;
    protected readonly ushort Port;

    protected BLiveServer()
    {
        (Enable, Port) = GetServerConfigBase();
    }

    internal event ServerEventHandler? ServerStateChange;
    internal event ServerEventHandler? ClientCountChange;

    private (bool, ushort) GetServerConfigBase()
    {
        return GetServerConfig();
    }

    protected abstract (bool, ushort) GetServerConfig();


    protected async void HandleClient(Guid clientId)
    {
        while (await SendHeartbeatToClient(clientId)) await Task.Delay(1000);
    }

    private async Task<bool> SendHeartbeatToClient(Guid clientId)
    {
        return await SendToClient(clientId, BLiveBase.CreateHeartbeatPacket(0));
    }

    internal void SendSmsToClients(byte[] rawData)
    {
        if (!Enable) return;
        foreach (var clientId in Clients.Keys) SendSmsToClient(clientId, rawData);
    }

    private async void SendSmsToClient(Guid clientId, byte[] rawData)
    {
        await SendToClient(clientId, rawData);
    }

    private async Task<bool> SendToClient(Guid clientId, byte[] rawData)
    {
        if (!Clients.TryGetValue(clientId, out var clientSocket)) return false;
        try
        {
            await ClientSendAsync(clientSocket, rawData);
        }
        catch (Exception)
        {
            ClientDispose(clientSocket);
            Clients.TryRemove(clientId, out _);
            ClientCountChange?.Invoke($"{Clients.Count}");
            return false;
        }

        return true;
    }

    protected void InvokeClientCountChange(string result)
    {
        ClientCountChange?.Invoke(result);
    }

    protected void InvokeServerStateChange(string result)
    {
        ServerStateChange?.Invoke(result);
    }

    protected abstract void ClientDispose(T clientSocket);

    protected abstract Task ClientSendAsync(T clientSocket, byte[] rawData);

    internal delegate void ServerEventHandler(string result);
}