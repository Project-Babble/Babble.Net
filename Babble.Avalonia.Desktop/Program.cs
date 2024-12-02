using Avalonia;
using DesktopNotifications;
using DesktopNotifications.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Babble.Avalonia.Desktop;

class Program
{

    private static INotificationManager _notificationManager = null;

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

        if (_notificationManager is not null)
        {
            App.SendNotification += NotificationRequested;
            _notificationManager.NotificationActivated += OnNotificationActivated;
            _notificationManager.NotificationDismissed += OnNotificationDismissed;
        }

        return builder.StartWithClassicDesktopLifetime(args);
    }

    private static void NotificationRequested(
        string? title, 
        string? body, 
        string? bodyImagePath, 
        string bodyImageAltText, 
        List<(string Title, string ActionId)?> buttons,
        DateTimeOffset? deliveryTime,
        DateTimeOffset? expirationTime)
    {
        Notification notification = new();
        notification.Title = title;
        notification.Body = body;
        notification.BodyImagePath = bodyImagePath;
        notification.BodyImageAltText = bodyImageAltText;

        if (buttons is not null)
            notification.Buttons.AddRange(buttons.AsEnumerable().Select(x => x!.Value));

        if (deliveryTime.HasValue && expirationTime.HasValue)
            _notificationManager.ScheduleNotification(notification, deliveryTime.Value, expirationTime.Value);
        else if (deliveryTime.HasValue)
            _notificationManager.ScheduleNotification(notification, deliveryTime.Value);
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
