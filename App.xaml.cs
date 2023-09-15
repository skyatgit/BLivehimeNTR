using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace BLivehimeNTR;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private Mutex? _mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, "BLivehimeNTR", out var createdNew);

        if (!createdNew)
        {
            MessageBox.Show("不要重复启动本程序。");
            _mutex.Dispose();
            Current.Shutdown();
            return;
        }

        var x86TempFolderPath = Path.Combine(Path.GetTempPath(), "BLivehimeNTR", "x86");
        var x64TempFolderPath = Path.Combine(Path.GetTempPath(), "BLivehimeNTR", "x64");
        Environment.SetEnvironmentVariable("Path", $"{x86TempFolderPath};{Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process)}", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("Path", $"{x64TempFolderPath};{Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Process)}", EnvironmentVariableTarget.Process);
        Directory.CreateDirectory(x86TempFolderPath);
        Directory.CreateDirectory(x64TempFolderPath);
        var assembly = Assembly.GetExecutingAssembly();
        using var x86WpcapDll = assembly.GetManifestResourceStream("BLivehimeNTR.Npcap.x86.wpcap.dll");
        using var x86PacketDll = assembly.GetManifestResourceStream("BLivehimeNTR.Npcap.x86.Packet.dll");
        using var x64WpcapDll = assembly.GetManifestResourceStream("BLivehimeNTR.Npcap.x64.wpcap.dll");
        using var x64PacketDll = assembly.GetManifestResourceStream("BLivehimeNTR.Npcap.x64.Packet.dll");
        using var x86WpcapFileStream = new FileStream(Path.Combine(x86TempFolderPath, "wpcap.dll"), FileMode.Create);
        using var x86PacketFileStream = new FileStream(Path.Combine(x86TempFolderPath, "Packet.dll"), FileMode.Create);
        using var x64WpcapFileStream = new FileStream(Path.Combine(x64TempFolderPath, "wpcap.dll"), FileMode.Create);
        using var x64PacketFileStream = new FileStream(Path.Combine(x64TempFolderPath, "Packet.dll"), FileMode.Create);
        x86WpcapDll?.CopyTo(x86WpcapFileStream);
        x86PacketDll?.CopyTo(x86PacketFileStream);
        x64WpcapDll?.CopyTo(x64WpcapFileStream);
        x64PacketDll?.CopyTo(x64PacketFileStream);
        base.OnStartup(e);
    }
}