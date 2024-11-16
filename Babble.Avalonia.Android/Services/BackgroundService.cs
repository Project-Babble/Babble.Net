using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;

namespace Babble.Avalonia.Android.Services;

[Service(Exported = false)]
public class BackgroundService : Service
{
    private const int ServiceNotificationId = 1;
    private const string ChannelId = "BackgroundServiceChannel";
    private PowerManager.WakeLock _wakeLock;
    private bool _isRunning;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        _isRunning = true;

        // Create notification channel for Android O and above
        CreateNotificationChannel();

        // Start foreground service with notification
        StartForeground(ServiceNotificationId, CreateNotification());

        // Acquire wake lock
        AcquireWakeLock();

        // Return sticky to restart service if it gets killed
        return StartCommandResult.Sticky;
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                ChannelId,
                "Background Service",
                NotificationImportance.Low)
            {
                Description = "Keeps the app running in background"
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }

    private Notification CreateNotification()
    {
        var notificationIntent = new Intent(this, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(
            this,
            0,
            notificationIntent,
            PendingIntentFlags.Immutable);

        var notificationBuilder = new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle("App Running")
            .SetContentText("Your app is running in the background")
            .SetSmallIcon(Resource.Drawable.Icon)
            .SetOngoing(true)
            .SetContentIntent(pendingIntent);

        return notificationBuilder.Build();
    }

    private void AcquireWakeLock()
    {
        var powerManager = (PowerManager)GetSystemService(PowerService);
        _wakeLock = powerManager.NewWakeLock(
            WakeLockFlags.Partial,
            "YourApp:BackgroundServiceLock");
        _wakeLock.Acquire();
    }

    public override void OnDestroy()
    {
        _isRunning = false;
        _wakeLock?.Release();
        _wakeLock?.Dispose();
        base.OnDestroy();
    }
}