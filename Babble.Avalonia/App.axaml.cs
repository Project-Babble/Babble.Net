﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Localizer.Core;
using Avalonia.Markup.Xaml;
using Babble.Avalonia.ReactiveObjects;
using Babble.Avalonia.Scripts;
using Babble.Avalonia.ViewModels;
using Babble.Avalonia.Views;
using Babble.Core;
using Babble.OSC;
using Hypernex.ExtendedTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VRCFaceTracking;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Services;

namespace Babble.Avalonia;

public class App : Application
{
    public static Action<IServiceCollection>? RegisterPlatformService;
    
    // ReSharper disable once MemberCanBePrivate.Global
    public static IHost GlobalHost => Host.CreateDefaultBuilder()
        .ConfigureServices((services) =>
        {
            RegisterPlatformService?.Invoke(services);
            services.AddSingleton<CamView>();
            services.AddSingleton<AlgoView>();
            services.AddSingleton<SettingsView>();
            services.AddSingleton<CalibrationView>();
            services.RegisterSingleton<CamView, CamViewModel>();
            services.RegisterSingleton<AlgoView, AlgoSettingsViewModel>();
            services.RegisterSingleton<SettingsView, SettingsViewModel>();
            services.RegisterSingleton<CalibrationView, CalibrationViewModel>();
        }).Build();
    
    // Platform-agnostic Notification representation.
    // Implementers can subscribe to this Action when a notification is to be raised
    // Ex: SendNotification.Invoke("Title", "Body", string.Empty, string.Empty, null, null, null);
    public static event Action<                      
        string?,                                     // Title
        string?,                                     // Body
        string?,                                     // Body Image Path 
        string,                                      // Body Alt. Text
        List<(string Title, string ActionId)?>,      // Action Buttons
        DateTimeOffset?,                             // Optional: Scheduled time to send Notification
#pragma warning disable CS0067 // Event is never used
        DateTimeOffset?> SendNotification = null!;           // Optional: Expiration time for sent Notification
#pragma warning restore CS0067 // Event is never used

    private MainIntegrated _mainIntegrated = null!;
    private BabbleOSC _babbleOsc = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Start BabbleCore early so we can load settings.
        // In the VRCFT module, we skip init if we're already started
        BabbleCore.Instance.Start();
        var lang = BabbleCore.Instance.Settings.GeneralSettings.GuiLanguage;
        LocalizerCore.Localizer.SwitchLanguage(lang);

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
        ParameterSenderService.AllParametersRelevant = true; // Don't touch! We need

        // Setup custom OSC handler
        var ip = BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress;
        var port = BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort;
        _babbleOsc = new BabbleOSC(ip, port);
    }

    public override async void OnFrameworkInitializationCompleted()
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
        await GlobalHost.StartAsync();
    }

    private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _mainIntegrated.Teardown();
        _babbleOsc.Teardown();
    }

    private void OnShutdownClicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
