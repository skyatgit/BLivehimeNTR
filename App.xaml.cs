using System.Threading;
using System.Windows;

namespace BLivehimeNTR;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        _ = new Mutex(true, "BLivehimeNTR", out var createdNew);

        if (!createdNew)
        {
            MessageBox.Show("不要重复启动本程序。");
            Current.Shutdown();
            return;
        }

        base.OnStartup(e);
    }
}