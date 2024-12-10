using Avalonia;
using DesktopNotifications;
using DesktopNotifications.Avalonia;
using System;
using System.Linq;
using System.Threading;
using Babble.Avalonia.Scripts.Models;

namespace Babble.Avalonia.Desktop;

class Program
{
    private static INotificationManager _notificationManager = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        var builder = BuildAvaloniaApp();
        if(args.Contains("--drm"))
        {
            SilenceConsole();
                
            // If Card0, Card1 and Card2 all don't work. You can also try:                 
            // return builder.StartLinuxFbDev(args);
            // return builder.StartLinuxDrm(args, "/dev/dri/card1");
            return builder.StartLinuxDrm(args, "/dev/dri/card1", 1D);
        }

        App.SendNotification += NotificationRequested;
        _notificationManager.NotificationActivated += OnNotificationActivated;
        _notificationManager.NotificationDismissed += OnNotificationDismissed;

        return builder.StartWithClassicDesktopLifetime(args);
    }

    private static void NotificationRequested(NotificationModel notificationModel)
    {
        Notification notification = new()
        {
            Title = notificationModel.Title,
            Body = notificationModel.Body,
            BodyImagePath = notificationModel.BodyImagePath,
            BodyImageAltText = notificationModel.BodyAltText
        };

        if (notificationModel.ActionButtons is not null)
        {
            if (notificationModel.ActionButtons.Count != 0)
                notification.Buttons.AddRange(notificationModel.ActionButtons.
                    Where(x => x.HasValue).
                    Select(x => x!.Value));
        }
        
        if (notificationModel is { OptionalScheduledTime: not null, OptionalExpirationTime: not null })
            _notificationManager.ScheduleNotification(notification, notificationModel.OptionalScheduledTime.Value, notificationModel.OptionalExpirationTime.Value);
        else if (notificationModel.OptionalScheduledTime.HasValue)
            _notificationManager.ScheduleNotification(notification, notificationModel.OptionalScheduledTime.Value);
        else
            _notificationManager.ShowNotification(notification);
    }

    private static void OnNotificationActivated(object? sender, NotificationActivatedEventArgs e)
    {
        
    }

    private static void OnNotificationDismissed(object? sender, NotificationDismissedEventArgs e)
    {
        
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .SetupDesktopNotifications(out _notificationManager!)
            .WithInterFont()
            .LogToTrace();

    private static void SilenceConsole()
    {
        new Thread(() =>
            {
                Console.CursorVisible = false;
                while(true)
                    Console.ReadKey(true);
            })
            { IsBackground = true }.Start();
    }
}