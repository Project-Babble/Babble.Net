using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Babble.Avalonia.ViewModels;
using Babble.Avalonia.Views;
using Babble.Core;
using Babble.OSC;
using Meadow;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Babble.Avalonia;

public partial class App : AvaloniaMeadowApplication<Linux>
{
    private static readonly HashSet<string> Whitelist = ["gui_osc_location", "gui_osc_address", "gui_osc_port", "gui_osc_receiver_port"];
    private BabbleOSC _sender;
    private Thread _thread;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        LoadMeadowOS();

        BabbleCore.Instance.Start();
        BabbleCore.Instance.Settings.OnUpdate += NeedRestartOSC;

        var ip = BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress;
        var remotePort = BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort;
        _sender = new BabbleOSC(ip, remotePort);
        _thread = new Thread(new ThreadStart(OSCLoop));
        _thread.Start();
    }

    public override Task InitializeMeadow()
    {
        var r = Resolver.Services.Get<IMeadowDevice>();

        if (r == null)
        {
            Resolver.Log.Info("IMeadowDevice is null");
        }
        else
        {
            Resolver.Log.Info($"IMeadowDevice is {r.GetType().Name}");
        }

        return Task.CompletedTask;

    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new DesktopMainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            desktop.Startup += OnStartup;
            desktop.Exit += OnExit;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MobileMainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnStartup(object s, ControlledApplicationLifetimeStartupEventArgs e)
    {
        
    }

    private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _sender.Teardown();
        _thread.Join();
    }

    private void NeedRestartOSC(string name)
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
            Thread.Sleep(10);
        }
    }
}
