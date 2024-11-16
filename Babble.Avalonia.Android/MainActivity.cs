using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using Babble.Avalonia.Android.Services;

namespace Babble.Avalonia.Android;

[Activity(
    Label = "Babble App",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private bool _isServiceRunning;
    private Intent _backgroundServiceIntent;

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        _backgroundServiceIntent = new Intent(this, typeof(BackgroundService));
    }


    public void StartBackgroundService()
    {
        if (!_isServiceRunning)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                StartForegroundService(_backgroundServiceIntent);
            }
            else
            {
                StartService(_backgroundServiceIntent);
            }
            _isServiceRunning = true;
        }
    }

    public void StopBackgroundService()
    {
        if (_isServiceRunning)
        {
            StopService(_backgroundServiceIntent);
            _isServiceRunning = false;
        }
    }

    protected override void OnDestroy()
    {
        // Only stop the service if the app is actually being destroyed,
        // not just configured
        if (IsFinishing)
        {
            StopBackgroundService();
        }
        base.OnDestroy();
    }
}
