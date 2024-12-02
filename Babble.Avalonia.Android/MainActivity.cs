using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Android.OS.PowerManager;

namespace Babble.Avalonia.Android;

[Activity(
    Label = "Babble App",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/Icon_512x512",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private NotificationManagerService _notificationManagerService;
    private PowerManager _pmanager;
    private WakeLock _wakelock;

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        _pmanager = (PowerManager)GetSystemService(PowerService)!;
        _wakelock = _pmanager.NewWakeLock(WakeLockFlags.Partial, "babble-app")!;
        _wakelock.SetReferenceCounted(false);
        _wakelock.Acquire();

        _notificationManagerService = new NotificationManagerService();
        Task.Run(Permissions.RequestAsync<NotificationPermission>);
        App.SendNotification += NotificationRequested;

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    private void NotificationRequested(
        string? title,
        string? body,
        string? bodyImagePath,
        string bodyImageAltText,
        List<(string Title, string ActionId)?> buttons,
        DateTimeOffset? deliveryTime,
        DateTimeOffset? expirationTime)
    {
        if (deliveryTime is not null)
            _notificationManagerService.SendNotification(title, body, deliveryTime.Value.DateTime);
        else
            _notificationManagerService.SendNotification(title, body);
    }

    protected override void Dispose(bool disposing)
    {
        _wakelock.Release();
        _pmanager.Dispose();
        base.Dispose(disposing);
    }
}
