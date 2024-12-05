using System;
using System.Buffers.Binary;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Android.Hardware.Usb;
using Babble.Core;
using Babble.Core.Scripts.Captures;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using UsbSerialForAndroid.Net;
using UsbSerialForAndroid.Net.Drivers;
using UsbSerialForAndroid.Net.Enums;

namespace Babble.Avalonia.Android;

public class AndroidSerialCameraCapture : Capture
{
    public override string Url { get; set; }
    public override uint FrameCount { get; protected set; }
    public override Mat RawMat { get; }
    public override (int width, int height) Dimensions { get; }
    public override bool IsReady { get; protected set; }

    private const int BAUD_RATE = 3000000;
    private const ulong ETVR_HEADER = 0xd8ff0000a1ffa0ff, ETVR_HEADER_MASK = 0xffff0000ffffffff;
    private UsbDriverBase _currentUSB;
    
    public AndroidSerialCameraCapture(string Url) : base(Url)
    {
    }
    
    public override Task<bool> StartCapture()
    {
        UsbDriverFactory.RegisterUsbBroadcastReceiver(
            isShowToast: true,
            OnUsbDeviceAttached,
            OnUsbDeviceDetached);
        
        return Task.FromResult(true);
    }

    private void OnUsbDeviceAttached(UsbDevice usb)
    {
        if (true) // Whatever we plug in last, we use for now
        {
            _currentUSB.Open(BAUD_RATE, 8, StopBits.None, Parity.None);
            DataLoop();
        }
    }
    
    private async void DataLoop()
    {
        byte[] buffer = new byte[2048];
        try
        {
            while (_currentUSB.TestConnection())
            {
                Stream stream = new MemoryStream(_currentUSB.Read());
                for (int bufferPosition = 0; bufferPosition < sizeof(ulong);)
                    bufferPosition += await stream.ReadAsync(buffer, bufferPosition, sizeof(ulong) - bufferPosition);
                ulong header = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
                for (; (header & ETVR_HEADER_MASK) != ETVR_HEADER; header = header >> 8 | (ulong)buffer[0] << 56)
                    while (await stream.ReadAsync(buffer, 0, 1) == 0) /**/;

                ushort jpegSize = (ushort)(header >> BitOperations.TrailingZeroCount(~ETVR_HEADER_MASK));
                if (buffer.Length < jpegSize)
                    Array.Resize(ref buffer, jpegSize);

                BinaryPrimitives.WriteUInt16LittleEndian(buffer, 0xd8ff);
                for (int bufferPosition = 2; bufferPosition < jpegSize;)
                    bufferPosition += await stream.ReadAsync(buffer, bufferPosition, jpegSize - bufferPosition);
                Mat.FromImageData(buffer, ImreadModes.Color).CopyTo(RawMat);
                FrameCount++;
            }
        }
        catch (ObjectDisposedException ex)
        {
            // Handle when the device is unplugged
            StopCapture();

        }
        catch (Exception ex)
        {
            BabbleCore.Instance.Logger.LogError($"Error reading frame on port {Url}: {ex.Message}");
            StopCapture();
        }
    }
    
    private void OnUsbDeviceDetached(UsbDevice usb)
    {
        if (usb.DeviceId == _currentUSB.UsbDevice.DeviceId)
        {
            _currentUSB.Close();
        }
    }

    public override bool StopCapture()
    {
        UsbDriverFactory.UnRegisterUsbBroadcastReceiver();
        return true;
    }
}