using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Hardware.Usb;
using Android.Util;
using Babble.Core;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using static Android.OS.PowerManager;

[assembly: UsesFeature("android.hardware.usb.host")]

namespace Babble.Avalonia.Android;

[IntentFilter([UsbManager.ActionUsbDeviceAttached])]
[MetaData(UsbManager.ActionUsbDeviceAttached, Resource = "@xml/device_filter")]
[Activity(
    Label = "Babble App",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/Icon_512x512",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public partial class MainActivity : AvaloniaMainActivity<App>
{
    // Wake lock related stuff
    private NotificationManagerService _notificationManagerService;
    private PowerManager _pmanager;
    private WakeLock _wakelock;

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        // Request wake lock
        _pmanager = (PowerManager)GetSystemService(PowerService)!;
        _wakelock = _pmanager.NewWakeLock(WakeLockFlags.Partial, "babble-app")!;
        _wakelock.SetReferenceCounted(false);
        _wakelock.Acquire();
        
        // Setup notification service
        _notificationManagerService = new NotificationManagerService();
        Task.Run(Permissions.RequestAsync<NotificationPermission>);
        App.SendNotification += NotificationRequested;

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    protected override void OnStart()
    {
        base.OnStart();
        
        // Register our custom camera capture
        AndroidSerialCameraCapture.App = this;
        var tuple = (new HashSet<string>() { "/dev" }, false);
        BabbleCore.Instance.PlatformConnector.Captures.TryAdd(tuple, typeof(AndroidSerialCameraCapture));
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
        App.SendNotification -= NotificationRequested;
        _wakelock.Release();
        _pmanager.Dispose();
        base.Dispose(disposing);
    }
}
