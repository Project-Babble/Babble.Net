using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;
using Android.Views;
using Babble.Avalonia.Scripts.Models;

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
    private NotificationManagerService _notificationManagerService = null!;

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        // I cannot, for the life of me, create a partial wake on lock for this god-forsaken platform.
        // No, this is not a permissions issue. Name it, I've tried them all. Partial wake lock, full wake lock,
        // ignore battery optimization settings, etc.
        // Droidcam does it, Google maps can run on the lock screen, tf it going on here
        // Whatever. On the Quest the app runs in the background screen-on anyways, if you're running this on a 
        // Android phone just turn your brightness all the way down or something ¯\_(ツ)_/¯
        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);

        _notificationManagerService = new NotificationManagerService();
        Task.Run(Permissions.RequestAsync<NotificationPermission>);
        App.SendNotification += NotificationRequested;

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    private void NotificationRequested(NotificationModel notificationModel)
    {
        if (notificationModel.OptionalScheduledTime is not null)
            _notificationManagerService.SendNotification(
                notificationModel.Title!, 
                notificationModel.Body!, 
                notificationModel.OptionalScheduledTime.Value.DateTime);
        else
            _notificationManagerService.SendNotification(
                notificationModel.Title!, 
                notificationModel.Body!);
    }

    protected override void Dispose(bool disposing)
    {
        App.SendNotification -= NotificationRequested;
        base.Dispose(disposing);
    }
}