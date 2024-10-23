using Babble.Core;
using Babble.Maui.Locale;
using Babble.Maui.Scripts;
using Babble.Maui.Scripts.Decoders;
using Babble.OSC;

namespace Babble.Maui;

/// <summary>
/// The main entrypoint for our application.
/// </summary>
public partial class App : Application
{
    private readonly PlatformConnector _platformConnector;
    private readonly BabbleOSC _sender;
    private readonly Thread _thread;

    // Note: The EmguCV CUDA runtime BLOWS UP how much ram we use?? Like 150mb to 1.2GB!!
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
        
        // Debugging values
        const string _lang = "English";
        const string _ip = @"192.168.0.75";
        const string _ipCameraUrl = @$"http://{_ip}:4747/video";
        var randomCameraUrl = EmguCVDeviceEnumerator.EnumerateCameras(out var cameraMap) ?
            cameraMap.ElementAt(Random.Shared.Next(cameraMap.Count)).Key.ToString() : "0";

        LocaleManager.Initialize(_lang);
        
        _platformConnector = SetupPlatform(randomCameraUrl);
        _platformConnector.Initialize();

        // TODO Pass in user's Quest headset address here!
        _sender = new BabbleOSC(_ip);
        _thread = new Thread(new ThreadStart(Update));
        _thread.Start();
    }

    /// <summary>
    /// Creates the proper video streaming classes based on the platform we're deploying to.
    /// EmguCV doesn't have support for VideoCapture on Android, iOS, or UWP
    /// We have a custom implementations for IP Cameras, the de-facto use case on mobile
    /// As well as SerialCameras (not tested on mobile yet)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    private static PlatformConnector SetupPlatform(string url)
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.Unknown)
        {
            throw new PlatformNotSupportedException();
        }
        else if (DeviceInfo.Current.Platform == DevicePlatform.Android||
                 DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {

            return new MobileConnector(@"http://192.168.0.75:4747/video"); // url
        }
        else
        {
            // Else, for WinUI, macOS, watchOS, MacCatalyst, tvOS, Tizen, etc...
            // Use the standard EmguCV VideoCapture backend
            return new DesktopConnector(url);
        }
    }

    private void Update()
    {
        while (true)
        {
            var data = _platformConnector.GetFrameData();

            if (!BabbleCore.GetExpressionData(data, out var expressions))
                goto End;
            
            foreach (var exp in expressions)
                BabbleOSC.Expressions.SetByKey1(exp.Key, exp.Value);

            End:
            Thread.Sleep(Utils.THREAD_TIMEOUT_MS);
        }
    }
}
