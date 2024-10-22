using Babble.Core;
using Babble.Maui.Locale;
using Babble.Maui.Scripts;
using Babble.Maui.Scripts.Decoders;
using Babble.OSC;

namespace Babble.Maui;

public partial class App : Application
{
    private readonly PlatformConnector _platformConnector;
    private readonly BabbleOSC _sender;
    private readonly Thread _thread;

    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Note: Currently the app will hang until the IP Camera defined below connects
        // Note: The CUDA runtime BLOWS UP how much ram we use?? Like 150mb to 1.2GB!!
        const string _lang = "English";
        const string _ip = @"192.168.0.75";
        const string _ipCameraUrl = @$"http://{_ip}:4747/video";

        LocaleManager.Initialize(_lang);

        //var debugCamera = EmguCVDeviceEnumerator.EnumerateCameras(out var cameraMap) ?
        //    cameraMap.Last().Key.ToString() : "0";

        _platformConnector = SetupPlatform();
        _platformConnector.Initialize();

        // TODO Pass in user's Quest headset address here!
        _sender = new BabbleOSC(_ip);
        _thread = new Thread(new ThreadStart(Update));
        _thread.Start();
    }

    private static PlatformConnector SetupPlatform()
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.Unknown)
        {
            throw new PlatformNotSupportedException();
        }
        else if (DeviceInfo.Current.Platform == DevicePlatform.Android||
                 DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            // EmguCV doesn't have support for VideoCapture on Android, iOS, or UWP
            // So we have a custom implementation for IP Cameras, the de-facto use case on mobile
            return new MobileConnector(@"http://192.168.0.75:4747/video");
        }
        else
        {
            // Else, for WinUI, macOS, watchOS, MacCatalyst, tvOS, Tizen, etc...
            // Use the standard EmguCV VideoCapture backend
            return new DesktopConnector("0");
        }
    }

    private void Update()
    {
        while (true)
        {
            var data = _platformConnector.GetFrameData();

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
