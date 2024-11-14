using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Localizer.Core;
using Avalonia.Markup.Xaml;
using Babble.Avalonia.ViewModels;
using Babble.Avalonia.Views;
using Babble.Core;
using Babble.OSC;
using Hypernex.ExtendedTracking;
using Meadow;
using Microsoft.Extensions.Logging;
using VRCFaceTracking;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Services;

namespace Babble.Avalonia;

public partial class App : AvaloniaMeadowApplication<Linux>
{
    internal static ILogger Logger { get; private set; }

    private Task _task;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private CancellationToken _cancellationToken;
    private MainIntegrated _mainIntegrated;
    private BabbleOSC _babbleOSC;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        LoadMeadowOS();

        // Start BabbleCore early so we can load settings.
        // In the VRCFT module, we skip init if we're already started
        BabbleCore.Instance.Start();
        var lang = BabbleCore.Instance.Settings.GeneralSettings.GuiLanguage;
        LocalizerCore.Localizer.SwitchLanguage(lang);

        // Setup logging
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = factory.CreateLogger("Babble App");

        // Setup VRCFT
        var settings = new FaceTrackingServices.FTSettings();
        var loggerFactory = new FaceTrackingServices.FTLoggerFactory();
        var dispatcher = new FaceTrackingServices.FTDispatcher();
        var moduleDataServiceLogger = loggerFactory.CreateLogger<ModuleDataService>();
        var mutatorLogger = loggerFactory.CreateLogger<UnifiedTrackingMutator>();
        var moduleDataService = new ModuleDataService(new FaceTrackingServices.BabbleIdentity(), moduleDataServiceLogger);
        var libManager = new UnifiedLibManager(loggerFactory, dispatcher, moduleDataService);
        var mutator = new UnifiedTrackingMutator(mutatorLogger, settings);
        _mainIntegrated = new MainIntegrated(loggerFactory, libManager, mutator);
        Task.Run(async () => await _mainIntegrated.InitializeAsync());
        ParameterSenderService.AllParametersRelevant = true;

        // Setup custom OSC handler
        var ip = BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress;
        var port = BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort;
        _babbleOSC = new BabbleOSC(ip, port);
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
            desktop.Exit += Desktop_Exit;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MobileMainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            // We don't need to worry about teardown on mobile
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _mainIntegrated.Teardown();
        _babbleOSC.Teardown();
    }
}
