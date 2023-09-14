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

        base.OnStartup(e);
    }
}