using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Serial Camera capture class intended for use on Desktop platforms
/// Babble-board specific implementation, assumes a fixed camera size of 240x240
/// </summary>
public class SerialCameraCapture : Capture, IDisposable
{
    public override uint FrameCount { get; protected set; }

    private const int BAUD_RATE = 3000000;
    private static readonly byte[] ETVR_HEADER = { 0xff, 0xa0, 0xff, 0xa1 };       // xlinka 11/8/24: Changed to use array initializer
    private const int ETVR_HEADER_LEN = 6;                                         // 2 bytes header + 2 bytes frame type + 2 bytes size

    private readonly SerialPort _serialPort;
    private byte[] _buffer = new byte[2048];
    private int _bufferPosition;
    private bool _isDisposed;

    public override string Url { get; set; } = null!;
    public override Mat RawMat { get; } = new Mat();
    public override (int width, int height) Dimensions => (240, 240);
    public override bool IsReady { get; protected set; }

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public SerialCameraCapture(string portName) : base(portName)
    {
        _serialPort = new SerialPort
        {
            PortName = portName,
            BaudRate = BAUD_RATE,
            ReadTimeout = 1000,
            WriteTimeout = 1000
        };
    }

    public override Task<bool> StartCapture()
    {
        try
        {
            _serialPort.Open();
            Task.Run(GetNextFrame, _cancellationTokenSource.Token);
            IsReady = true;
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            BabbleCore.Instance.Logger.LogError($"Failed to open serial port {Url}: {ex.Message}"); // xlinka 11/8/24: Improved logging.
            IsReady = false;
            return Task.FromResult(false);
        }
    }

    public override bool StopCapture()
    {
        try
        {
            _cancellationTokenSource.Cancel();
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

    private Task GetNextFrame()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (!IsReady || !_serialPort.IsOpen) continue;

            try
            {
                if (_serialPort.BytesToRead > 0)
                {
                    var (start, jpegSize) = GetNextPacketBounds();
                    if (start == -1 || jpegSize == -1) continue;

                    byte[] jpegData = new byte[jpegSize];
                    Array.Copy(_buffer, start + ETVR_HEADER_LEN, jpegData, 0, jpegSize);

                    if (jpegData.Length >= 2 && jpegData[0] == 0xFF && jpegData[1] == 0xD8) // xlinka 11/8/24: Check for valid JPEG header
                    {
                        _bufferPosition = 0;
                        CvInvoke.Imdecode(jpegData, ImreadModes.Color, RawMat);
                        FrameCount++;
                        continue;
                    }

                    // xlinka 11/8/24: Clear buffer after processing each frame to prevent leftover data
                    Array.Clear(_buffer, 0, _buffer.Length);
                    _bufferPosition = 0;
                }
            }
            catch (Exception ex)
            {
                IsReady = false;
                _serialPort.Close();
                BabbleCore.Instance.Logger.LogError($"Error reading frame on port {Url} at buffer position {_bufferPosition}: {ex.Message}"); // xlinka 11/8/24: Added detailed logging for frame reading errors
            }

            Task.Delay(100);
        }

        return Task.CompletedTask;
    }

    private (int start, int size) GetNextPacketBounds()
    {
        int headerPos = -1;

        // Keep reading until we find a valid header
        while (headerPos == -1)
        {
            // xlinka 11/8/24: Read data into buffer, respecting current buffer position to avoid overwriting data
            int bytesRead = _serialPort.Read(_buffer, _bufferPosition, Math.Min(2048, _buffer.Length - _bufferPosition));
            
            if (bytesRead == 0)
            {
                return (-1, -1);
            }

            _bufferPosition += bytesRead;

            // Search for the protocol header
            for (int i = 0; i <= _bufferPosition - ETVR_HEADER.Length; i++)
            {
                if (CompareBytes(_buffer, i, ETVR_HEADER))
                {
                    headerPos = i;
                    break;
                }
            }
        }

        // Extract the JPEG data size from the header
        int jpegSize = BitConverter.ToUInt16(_buffer, headerPos + 4); // xlinka 11/8/24: Read size directly after header

        // Ensure buffer has enough space for JPEG data
        // Adjusted buffer resizing logic in GetNextPacketBounds to dynamically expand the buffer if the JPEG data size exceeds its current capacity. This ensures sufficient buffer space without truncating data.
        if (_buffer.Length < headerPos + ETVR_HEADER_LEN + jpegSize)
        {
            Array.Resize(ref _buffer, headerPos + ETVR_HEADER_LEN + jpegSize); // xlinka 11/8/24: Resize buffer if necessary
        }

        // Read any remaining data for the complete JPEG
        while (_bufferPosition < headerPos + ETVR_HEADER_LEN + jpegSize)
        {
            int bytesRead = _serialPort.Read(_buffer, _bufferPosition, Math.Min(2048, headerPos + ETVR_HEADER_LEN + jpegSize - _bufferPosition));
            _bufferPosition += bytesRead;
        }

        return (headerPos, jpegSize);
    }

    private bool CompareBytes(byte[] buffer, int start, byte[] pattern)
    {
        if (start + pattern.Length > buffer.Length) return false;

        for (int i = 0; i < pattern.Length; i++)
        {
            if (buffer[start + i] != pattern[i]) return false;
        }
        return true;
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
