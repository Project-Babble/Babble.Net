using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System.Buffers.Binary;
using System.IO.Ports;
using System.Numerics;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Serial Camera capture class intended for use on Desktop platforms
/// Babble-board specific implementation, assumes a fixed camera size of 240x240
/// </summary>
public class SerialCameraCapture : Capture, IDisposable
{
    public override uint FrameCount { get; protected set; }

    private const int BAUD_RATE = 3000000;
    private const ulong ETVR_HEADER = 0xd8ff0000a1ffa0ff, ETVR_HEADER_MASK = 0xffff0000ffffffff;

    private readonly SerialPort _serialPort;
    private bool _isDisposed;

    public override string Url { get; set; } = null!;
    public override Mat RawMat { get; } = new Mat();

    public override (int width, int height) Dimensions => (RawMat.Width, RawMat.Height);

    public override bool IsReady { get; protected set; }

    public SerialCameraCapture(string portName) : base(portName)
    {
        _serialPort = new SerialPort
        {
            PortName = portName,
            BaudRate = BAUD_RATE,
            ReadTimeout = SerialPort.InfiniteTimeout,
        };
    }

    public override Task<bool> StartCapture()
    {
        try
        {
            _serialPort.Open();
            IsReady = true;
            DataLoop();
        }
        catch (Exception ex)
        {
            BabbleCore.Instance.Logger.LogError($"Failed to open serial port {Url}: {ex.Message}"); // xlinka 11/8/24: Improved logging.
            IsReady = false;
        }
        return Task.FromResult(IsReady);
    }

    public override bool StopCapture()
    {
        try
        {
            _serialPort.Close();
            IsReady = false;
            return true;
        }
        catch (Exception ex)
        {
            BabbleCore.Instance.Logger.LogError($"Failed to close serial port {Url}: {ex.Message}"); // xlinka 11/8/24: Improved logging.
            return false;
        }
    }

    private async void DataLoop()
    {
        byte[] buffer = new byte[2048];
        try
        {
            while (_serialPort.IsOpen)
            {
                Stream stream = _serialPort.BaseStream;
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
                Mat.FromImageData(buffer).CopyTo(RawMat);
                FrameCount++;
            }
        }
        catch (Exception ex)
        {
            if (ex is ObjectDisposedException)
                return;
            BabbleCore.Instance.Logger.LogError($"Error reading frame on port {Url}: {ex.Message}");
            StopCapture();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                StopCapture(); // xlinka 11/8/24: Ensure capture stops before disposing resources
                _serialPort?.Dispose(); // xlinka 11/8/24: Dispose of serial port if initialized
            }
            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // xlinka 11/8/24: Suppress finalization as resources are now disposed
    }

    ~SerialCameraCapture()
    {
        Dispose(false);
    }
}
