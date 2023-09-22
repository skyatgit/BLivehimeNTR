using System.Linq;
using Conesoft.Network_Connections;
using SharpPcap;
using SharpPcap.LibPcap;

namespace BLivehimeNTR;

public static class BLivehimeNtrBase
{
    public static void SetFilterByConnection(this LibPcapLiveDevice device, Connection connection)
    {
        if (!device.Opened) device.Open();
        device.Filter = $"tcp and src host {connection.Remote.Address} and src port {connection.Remote.Port} and dst host {connection.Local.Address} and dst port {connection.Local.Port}";
    }

    public static LibPcapLiveDevice GetDevice(this Connection connection)
    {
        return LibPcapLiveDeviceList.Instance.FirstOrDefault(device => device.Addresses.FirstOrDefault(address => Equals(address.Addr.ipAddress, connection.Local.Address)) is not null)!;
    }

    public static bool Equal(this Connection? connA, Connection? connB)
    {
        return connA?.Local == connB?.Local && connA?.Remote == connB?.Remote;
    }
}