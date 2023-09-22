using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
    private readonly BLiveTcpServer _bLiveTcpServer = new();
    private readonly BLiveWebSocketServer _bLiveWebSocketServer = new();
    private readonly List<byte> _buffer = new();
    private readonly bool _running;
    private Connection? _connection;
    private LibPcapLiveDevice? _device;

    public MainWindow()
    {
        if (Environment.ExitCode == 1) return;
        InitializeComponent();
        _running = true;
        _api.DanmuMsg += DanmuMsgEvent;
        _api.OpSendSmsReply += OpSendSmsReplyEvent;
        _bLiveTcpServer.ServerStateChange += ServerStateChange;
        _bLiveTcpServer.ClientCountChange += TcpClientCountChange;
        _bLiveWebSocketServer.ServerStateChange += ServerStateChange;
        _bLiveWebSocketServer.ClientCountChange += WebSocketClientCountChange;
        _bLiveTcpServer.StartAsync();
        _bLiveWebSocketServer.StartAsync();
        ScanBLivehimeProcess();
    }


    private void Open_Click(object sender, RoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void WebSocketClientCountChange(string result)
    {
        WebSocketClientCount.Content = $"WebSocket客户端:{result}";
    }

    private void TcpClientCountChange(string result)
    {
        TcpClientCount.Content = $"Tcp客户端:{result}";
    }

    private void ServerStateChange(string result)
    {
        MsgBox.AppendText($"{result}\n");
    }

    [TargetCmd("ALL")]
    private void OpSendSmsReplyEvent(object sender, (string cmd, string hitCmd, JObject jsonRawData, byte[] rawData) e)
    {
        var data = BLiveBase.CreateSmsPacket(0, e.rawData);
        _bLiveWebSocketServer.SendSmsToClients(data);
        _bLiveTcpServer.SendSmsToClients(data);
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

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (_connection is null) return;
        e.Cancel = true;
        Hide();
    }
}