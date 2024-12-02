using Android.Content;

namespace Babble.Avalonia.Android;

[BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
public class AlarmHandler : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent?.Extras != null)
        {
            string title = intent.GetStringExtra(NotificationManagerService.TitleKey);
            string message = intent.GetStringExtra(NotificationManagerService.MessageKey);

            NotificationManagerService manager = NotificationManagerService.Instance ?? new NotificationManagerService();
            manager.SendNotification(title, message);
        }
    }
}
