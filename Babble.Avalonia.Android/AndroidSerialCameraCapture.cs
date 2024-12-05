using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.Util;
using Avalonia.Android;
using Babble.Core.Scripts.Captures;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Extensions;
using OpenCvSharp;

[assembly: UsesFeature("android.hardware.usb.host")]

namespace Babble.Avalonia.Android;

public class AndroidSerialCameraCapture : Capture
{
    public static AvaloniaMainActivity App { get; set; }

    public override string Url { get; set; }
    public override uint FrameCount { get; protected set; }
    public override bool IsReady { get; protected set; }
    public override Mat RawMat { get; }
    public override (int width, int height) Dimensions
    {
        get
        {
            if (RawMat.Width != 0 && RawMat.Height != 0)
            {
                return (RawMat.Width, RawMat.Height);
            }
            else
            {
                return (0, 0);
            }
        }
    }
    
    private const int BAUD_RATE = 3000000;
    private static readonly string TAG = typeof(MainActivity).Name;
    private SerialInputOutputManager _serialIoManager;
    private UsbManager usbManager;
    
    public AndroidSerialCameraCapture(string Url) : base(Url)
    {
    }
    
    public override async Task<bool> StartCapture()
    {
        usbManager = (UsbManager)App.GetSystemService("usb")!; // UsbService
        
        var portInfo = App.Intent.GetParcelableExtra(TAG) as UsbSerialPortInfo;
        int vendorId = portInfo.VendorId;
        int deviceId = portInfo.DeviceId;
        int portNumber = portInfo.PortNumber;

        Log.Info(TAG, $"VendorId: {vendorId} DeviceId: {deviceId} PortNumber: {portNumber}");

        var drivers = await FindAllDriversAsync(usbManager);
        var driver = drivers.FirstOrDefault(d => d.Device.VendorId == vendorId && d.Device.DeviceId == deviceId);
        if (driver == null)
            throw new Exception("Driver specified in extra tag not found.");

        var port = driver.Ports[portNumber];
        if (port == null)
        {
            Log.Error(TAG, "No serial device.");
            return false;
        }
        Log.Info(TAG, "Port: " + port);

        _serialIoManager = new SerialInputOutputManager(port)
        {
            BaudRate = BAUD_RATE,
            DataBits = 8,
            StopBits = StopBits.One,
            Parity = Parity.None,
        };
        
        _serialIoManager.DataReceived += (sender, e) => {
            App.RunOnUiThread(() => {
                UpdateReceivedData(e.Data);
            });
        };
        
        _serialIoManager.ErrorReceived += (sender, e) => {
            App.RunOnUiThread(() => {
                var intent = new Intent(App, typeof(MainActivity));
                App.StartActivity(intent);
            });
        };

        Log.Info(TAG, "Starting USB IO manager...");
        try
        {
            _serialIoManager.Open(usbManager);
            IsReady = true;
        }
        catch (Java.IO.IOException e)
        {
            IsReady = false;
            // Ignore
        }

        return IsReady;
    }

    public override bool StopCapture()
    {
        if (_serialIoManager != null && _serialIoManager.IsOpen)
        {
            IsReady = false;
            Log.Info(TAG, "Stopping IO manager...");
            try
            {
                _serialIoManager.Close();
            }
            catch (Java.IO.IOException)
            {
                // Ignore
            }
        }

        return true;
    }
    
    private Task<IList<IUsbSerialDriver>> FindAllDriversAsync(UsbManager usbManager)
    {
        // Use the default probe table. We can add entries to this as we wish.
        var usbProber = new UsbSerialProber(UsbSerialProber.DefaultProbeTable);
        return usbProber.FindAllDriversAsync(usbManager);
    }

    private void UpdateReceivedData(byte[] data)
    {
        Mat.FromImageData(data, ImreadModes.Color).CopyTo(RawMat);
        FrameCount++;
    }
}