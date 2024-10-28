using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Babble.Avalonia.ViewModels;
using Babble.Avalonia.Views;
using Babble.Core;
using Babble.Core.Scripts;
using Babble.OSC;
using System.Collections.Generic;
using System.Threading;

namespace Babble.Avalonia;

public partial class App : Application
{
    private static readonly HashSet<string> Whitelist = ["gui_osc_location", "gui_osc_address", "gui_osc_port", "gui_osc_receiver_port"];
    private BabbleOSC _sender;
    private Thread _thread;

    public App()
    {
        BabbleCore.Instance.Start();
        BabbleCore.Instance.Settings.OnUpdate += OnSettingsUpdate;

        var ip = BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress;
        var remotePort = BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort;
        _sender = new BabbleOSC(ip, remotePort);
        _thread = new Thread(new ThreadStart(OSCLoop));
        _thread.Start();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnSettingsUpdate(string name)
    {
        if (Whitelist.Contains(name))
        {
            _sender.Teardown();
            _thread.Join();
            var ip = BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress;
            var remotePort = BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort;
            _sender = new BabbleOSC(ip, remotePort);
            _thread = new Thread(new ThreadStart(OSCLoop));
            _thread.Start();
        }
    }

    private void OSCLoop()
    {
        while (true)
        {
            if (!BabbleCore.Instance.GetExpressionData(out var expressions))
                goto End;

            foreach (var exp in expressions)
                BabbleOSC.Expressions.SetByKey1(exp.Key, exp.Value);

            End:
            Thread.Sleep(Utils.THREAD_TIMEOUT_MS);
        }
    }
}
