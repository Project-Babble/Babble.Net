using Babble.Core;
using Babble.Maui.Locale;
using Babble.Maui.Scripts;
using Babble.Maui.Scripts.Platforms;
using Babble.OSC;

namespace Babble.Maui;

public partial class App : Application
{
    private IPlatformConnector _platformConnector;
    private readonly BabbleOSC _sender;
    private readonly Thread _thread;

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Note: Currently the app will hang until the IP Camera defined below connects
        const string _lang = "English";
        const string _ip = @"192.168.0.75";
        const string _ipCameraUrl = @$"http://{_ip}:4747/video";

        LocaleManager.Initialize(_lang);
        
        _platformConnector = DoPlatformSetup();
        if (!_platformConnector.Initialize(_ipCameraUrl))
        {
            throw new Exception("Failed to start platform connectors.");
        }
        if (!BabbleCore.StartInference())
        {
            throw new Exception("Failed to start Babble inference.");
        }

        // TODO Pass in user's Quest headset address here!
        _sender = new BabbleOSC(_ip);
        _thread = new Thread(new ThreadStart(Update));
        _thread.Start();
    }

    private IPlatformConnector DoPlatformSetup()
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.Unknown)
        {
            throw new PlatformNotSupportedException();
        }
        else if (DeviceInfo.Current.Platform == DevicePlatform.WinUI ||
                 DeviceInfo.Current.Platform == DevicePlatform.macOS)
        {
            return new DesktopConnector();
        }
        else
        {
            // Android, watchOS, iOS, MacCatalyst, tvOS, Tizen, etc...
            return new MobileConnector();
        }
    }

    private void Update()
    {
        while (true)
        {
            if (!_platformConnector.GetCameraData(out float[] data))
                goto End;

            if (!BabbleCore.GetExpressionData(data, out var expressions))
                goto End;

            // Assign Babble.Core data to Babble.Osc
            foreach (var exp in expressions)
                BabbleOSC.Expressions.SetByKey1(exp.Key, exp.Value);

            End:
            Thread.Sleep(Utils.THREAD_TIMEOUT_MS);
        }
    }
}
