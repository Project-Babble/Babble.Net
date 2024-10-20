using Babble.Core;
using Camera.MAUI;
using Rug.Osc;
using System.Net;

namespace Babble.Maui;

public partial class MainPage : TabbedPage
{
    private OscSender _sender;

    private bool _loop = true;

    private readonly Thread _thread;

    private readonly int _resolvedPort;

    private readonly string _resolvedHost;

    public const string DEFAULT_HOST = "127.0.0.1";

    public const int DEFAULT_PORT = 8888;

    private const int TIMEOUT_MS = 10000;

    private CameraView _capture;

    public MainPage()
    {
        InitializeComponent();

        // TODO Load from saved config
        _resolvedHost = DEFAULT_HOST;
        _resolvedPort = DEFAULT_PORT;

        ConfigureReceiver();
        _loop = true;
        _thread = new Thread(new ThreadStart(SendLoop));
        _thread.Start();

        BabbleCore.StartInference();
    }

    private void cameraView_CamerasLoaded(object sender, EventArgs e)
    {
        if (cameraView.Cameras.Count > 0)
        {
            _capture.Camera = cameraView.Cameras.First();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
            });
        }
    }


    // https://stackoverflow.com/a/62112246/1836461
    private static async Task<float[]> ImageSourceToFloatArray(ImageSource imgsrc)
    {
        // Get the stream from the ImageSource
        Stream stream = await ((StreamImageSource)imgsrc).Stream(CancellationToken.None);

        // Create a byte array to hold the image data
        byte[] bytesAvailable = new byte[stream.Length];
        stream.Read(bytesAvailable, 0, bytesAvailable.Length);

        // Create a float array to store the float representation of the byte data
        float[] floatArray = new float[bytesAvailable.Length];

        // Convert the byte data (0-255) to float (0.0-1.0)
        for (int i = 0; i < bytesAvailable.Length; i++)
        {
            floatArray[i] = bytesAvailable[i] / 255f;
        }

        return floatArray;
    }

    private void ConfigureReceiver()
    {
        IPAddress address = IPAddress.Parse(_resolvedHost);
        _sender = new OscSender(address, _resolvedPort);
        _sender.DisconnectTimeout = TIMEOUT_MS;
        _sender.Connect();
    }

    private async void SendLoop()
    {
        const int SLEEP_TIME = 100;
        while (true)
        {
            // Crunch the numbers

            float[] data = await ImageSourceToFloatArray(_capture.GetSnapShot());
            if (!BabbleCore.GetExpressionData(data, out var exp))
                goto End;

            try
            {
                switch (_sender.State)
                {
                    // Send OSC if we can
                    case OscSocketState.Connected:
                        foreach (var item in exp.OrderByDescending(x => x.Value))
                            _sender.Send(new OscMessage($"/{item.Key.ToString()}", item.Value));
                        break;
                    // Reconnect
                    case OscSocketState.NotConnected:
                    case OscSocketState.Closed:
                        _sender.Close();
                        _sender.Dispose();
                        ConfigureReceiver();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }

        End:
            Thread.Sleep(SLEEP_TIME);
        }
    }

    public void Teardown()
    {
        BabbleCore.StopInference();
        _loop = false;
        _sender.Close();
        _sender.Dispose();
        _thread.Join();
    }
}
