using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using System;

namespace Babble.Avalonia.Android;

[Activity(
    Label = "Babble App",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    private PowerManager.WakeLock _wakeLock;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Initialize our (partial) wake lock
        InitializeWakeLock();
    }

    private void InitializeWakeLock()
    {
        try
        {
            var powerManager = (PowerManager)GetSystemService(PowerService)!;
            _wakeLock = powerManager.NewWakeLock(
                WakeLockFlags.Partial,
                $"{PackageName}:WakeLock")!;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing wake lock: {ex}");
        }
    }

    public void AcquireWakeLock()
    {
        try
        {
            if (_wakeLock != null && !_wakeLock.IsHeld)
            {
                _wakeLock.Acquire();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error acquiring wake lock: {ex}");
        }
    }

    public void ReleaseWakeLock()
    {
        try
        {
            if (_wakeLock != null && _wakeLock.IsHeld)
            {
                _wakeLock.Release();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error releasing wake lock: {ex}");
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        // If we need to reacquire wake lock
        AcquireWakeLock();
    }

    protected override void OnPause()
    {
        if (!IsFinishing)
        {
            // App is just paused, might want to keep wake lock
        }
        else
        {
            ReleaseWakeLock();
        }

        base.OnPause();
    }

    protected override void OnDestroy()
    {
        CleanupResources();
        base.OnDestroy();
    }

    private void CleanupResources()
    {
        try
        {
            ReleaseWakeLock();
            _wakeLock?.Dispose();
            _wakeLock = null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cleaning up resources: {ex}");
        }
    }
}
