using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using SharpPcap.LibPcap;

namespace BLivehimeNTR;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private Mutex? _mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _mutex = new Mutex(true, "BLivehimeNTR", out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show("不要重复启动本程序。");
            AppShutdown();
            return;
        }

        if (NpcapInstalled()) return;
        if (MessageBox.Show("需要安装Npcap才能使用本程序,是否继续安装?", "安装Npcap", MessageBoxButton.YesNo) is MessageBoxResult.No)
        {
            AppShutdown();
            return;
        }

        var filePath = Path.Combine(Path.GetTempPath(), "npcap-1.76.exe");
        var assembly = Assembly.GetExecutingAssembly();
        using var file = assembly.GetManifestResourceStream("BLivehimeNTR.Npcap.npcap-1.76.exe");
        using var fileStream = new FileStream(filePath, FileMode.Create);
        file?.CopyTo(fileStream);
        fileStream.Dispose();
        file?.Dispose();
        var process = Process.Start(filePath);
        process.WaitForExit();
        if (process.ExitCode != 0) AppShutdown();
    }

    private static bool NpcapInstalled()
    {
        try
        {
            return LibPcapLiveDeviceList.Instance is not null;
        }
        catch
        {
            return false;
        }
    }

    private void AppShutdown()
    {
        _mutex?.Dispose();
        Current.Shutdown(1);
    }
}