using Babble.Core;
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

        const string _ipCameraUrl = @"http://192.168.0.173:4747/video";

        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI ||
            DeviceInfo.Current.Platform == DevicePlatform.macOS)
        {
            _platformConnector = new DesktopConnector();
        }
        else if (DeviceInfo.Current.Platform == DevicePlatform.Unknown)
        {
            throw new PlatformNotSupportedException();
        }
        else
        {
            // DeviceInfo.Current.Platform == DevicePlatform.Android
            // DeviceInfo.Current.Platform == DevicePlatform.watchOS
            // DeviceInfo.Current.Platform == DevicePlatform.iOS
            // DeviceInfo.Current.Platform == DevicePlatform.Tizen
            // DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst
            // DeviceInfo.Current.Platform == DevicePlatform.tvOS
            _platformConnector = new MobileConnector();
        }

        if (!_platformConnector.Initialize(_ipCameraUrl))
        {
            throw new Exception();
        }

        if (!BabbleCore.StartInference())
        {
            throw new Exception();
        }

        // TODO Pass in user's Quest headset address here!
        _sender = new BabbleOSC();
        _thread = new Thread(new ThreadStart(Update));
        _thread.Start();
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
