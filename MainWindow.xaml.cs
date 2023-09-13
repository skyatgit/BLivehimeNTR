using System;
using System.Linq;
using System.Threading.Tasks;
using Conesoft.Network_Connections;
using SharpPcap;
using SharpPcap.LibPcap;

namespace BLivehimeNTR;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly bool _running;
    private Connection? _connection;
    private LibPcapLiveDevice? _device;

    public MainWindow()
    {
        InitializeComponent();
        _running = true;
        ScanBLivehimeProcess();
    }

    private async void ScanBLivehimeProcess()
    {
        while (_running)
        {
            var connection = GetBLivehimeProcess();
            if (!_connection.Equal(connection))
            {
                _device?.Dispose();
                _device = connection?.GetDevice();
                _device?.SetFilterByConnection(connection!);
                if (_device != null) _device.OnPacketArrival += OnPacketArrivalEvent;
                _device?.StartCapture();
                _connection = connection;
            }

            await Task.Delay(1000);
        }
    }

    private static Connection? GetBLivehimeProcess()
    {
        return Connections.All.FirstOrDefault(connection => connection.Remote.Port == 2243 && connection.ProcessName == "livehime");
    }


    private void OnPacketArrivalEvent(object sender, PacketCapture e)
    {
        Console.WriteLine(e.GetPacket().GetPacket());
    }
}

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