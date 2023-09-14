using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using BLiveAPI;
using Conesoft.Network_Connections;
using Newtonsoft.Json.Linq;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace BLivehimeNTR;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly BLiveApi _api = new();
    private readonly List<byte> _buffer = new();
    private readonly bool _running;
    private Connection? _connection;
    private LibPcapLiveDevice? _device;

    public MainWindow()
    {
        InitializeComponent();
        _running = true;
        _api.DanmuMsg += DanmuMsgEvent;
        _api.OpSendSmsReply += OpSendSmsReplyEvent;
        ScanBLivehimeProcess();
    }

    [TargetCmd("OTHERS")]
    private void OpSendSmsReplyEvent(object sender, (string cmd, string hitCmd, JObject jsonRawData, byte[] rawData) e)
    {
        Console.WriteLine($"{e.cmd}:{e.hitCmd}");
        MsgBox.Text += $"{e.cmd}:{e.hitCmd}\n";
    }

    private void DanmuMsgEvent(object sender, (string msg, ulong userId, string userName, int guardLevel, string face, JObject jsonRawData, byte[] rawData) e)
    {
        Console.WriteLine($"{e.userName}:{e.msg}");
        MsgBox.Text += $"{e.userName}:{e.msg}\n";
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
                BLivehimeStatus.Content = $"🔴 {(connection is null ? "未" : "已")}连接到直播姬";
                BLivehimeStatus.Foreground = connection is null ? Brushes.Red : Brushes.LimeGreen;
            }

            await Task.Delay(200);
        }
    }

    private static Connection? GetBLivehimeProcess()
    {
        return Connections.All.FirstOrDefault(connection => connection.Remote.Port == 2243 && connection.ProcessName == "livehime");
    }


    private void OnPacketArrivalEvent(object sender, PacketCapture e)
    {
        var ethPacket = e.GetPacket().GetPacket();
        var internetPacket = ethPacket.PayloadPacket;
        var transmissionPacket = internetPacket.PayloadPacket;
        var dataPacketSegment = transmissionPacket.PayloadDataSegment;
        var push = (transmissionPacket as TcpPacket)!.Push;
        var finished = (transmissionPacket as TcpPacket)!.Finished;
        if (finished)
        {
            _buffer.Clear();
            return;
        }

        if (!push) _buffer.Clear();
        _buffer.AddRange(dataPacketSegment.ActualBytes());
        if (!push) return;
        var buffer = _buffer.ToArray();
        _buffer.Clear();
        Dispatcher.Invoke(() => { _api.DecodePacket(buffer, false); });
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