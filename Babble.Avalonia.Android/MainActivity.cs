using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using Emgu.CV;
using static Android.OS.PowerManager;

namespace Babble.Avalonia.Android;

[Activity(
    Label = "Babble App",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private PowerManager _pmanager;
    private WakeLock _wakelock;

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        CvInvokeAndroid.Init();

        _pmanager = (PowerManager)GetSystemService(PowerService)!;
        _wakelock = _pmanager.NewWakeLock(WakeLockFlags.Partial, "babble-app")!;
        _wakelock.SetReferenceCounted(false);
        _wakelock.Acquire();

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    protected override void Dispose(bool disposing)
    {
        _wakelock.Release();
        _pmanager.Dispose();
        base.Dispose(disposing);
    }
}
