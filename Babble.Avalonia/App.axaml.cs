using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Localizer.Core;
using Avalonia.Markup.Xaml;
using Babble.Avalonia.ViewModels;
using Babble.Avalonia.Views;
using Babble.Core;
using Babble.OSC;
using Babble.OSC.Expressions;
using Meadow;
using Microsoft.Extensions.Logging;

namespace Babble.Avalonia;

public partial class App : AvaloniaMeadowApplication<Linux>
{
    private static readonly HashSet<string> Whitelist = ["guiosclocation", "guioscaddress", "guioscport", "guioscreceiverport"];
    private BabbleOSC _sender;

    private Task _task;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private CancellationToken _cancellationToken;
    internal ILogger Logger { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        LoadMeadowOS();

        BabbleCore.Instance.Start();
        var settings = BabbleCore.Instance.Settings;
        settings.OnUpdate += NeedRestartOSC;

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = factory.CreateLogger("Avalonia");

        var ip = settings.GeneralSettings.GuiOscAddress;
        var remotePort = settings.GeneralSettings.GuiOscPort;
        _sender = new BabbleOSC(ip, remotePort);
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;
        _task = Task.Run(OSCLoop, _cancellationToken);
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
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MobileMainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();

        var lang = BabbleCore.Instance.Settings.GeneralSettings.GuiLanguage;
        LocalizerCore.Localizer.SwitchLanguage(lang);
    }

    private void NeedRestartOSC(string name)
    {
        var normalizedSetting = name.ToLower().Replace("_", string.Empty);
        if (Whitelist.Contains(normalizedSetting))
        {
            _sender.Teardown();
            var ip = BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress;
            var remotePort = BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort;
            _sender = new BabbleOSC(ip, remotePort);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _task = Task.Run(OSCLoop, _cancellationToken);
        }
    }

    private async Task OSCLoop()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            if (BabbleCore.Instance.GetExpressionData(out var expressions))
            {
                foreach (var exp in expressions)
                {
                    UnifiedExpressionToFloatMapping.Expressions[exp.Key] = exp.Value;
                }
            }

            await Task.Delay(200);
        }
    }
}
