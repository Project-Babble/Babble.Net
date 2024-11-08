using Emgu.CV;
using Emgu.CV.CvEnum;
using Microsoft.Extensions.Logging;
using System.IO.Ports;

namespace Babble.Core.Scripts.Decoders;

/// <summary>
/// Serial Camera capture class intended for use on Desktop platforms
/// Babble-board specific implementation, assumes a fixed camera size of 256x256
/// </summary>
public class SerialCamera : Capture, IDisposable
{
    private const int BAUD_RATE = 3000000;
    private static readonly byte[] ETVR_HEADER = [0xff, 0xa0];
    private static readonly byte[] ETVR_HEADER_FRAME = [0xff, 0xa1];
    private const int ETVR_HEADER_LEN = 6;  // 2 bytes header + 2 bytes frame type + 2 bytes size

    private readonly SerialPort _serialPort;
    private byte[] _buffer = new byte[2048];
    private int _bufferPosition;
    private bool _isDisposed;

    public override string Url { get; set; }
    public override Mat RawFrame { get; set; }
    public override (int width, int height) Dimensions => (240, 240);
    public override bool IsReady { get; set; }

    public SerialCamera(string portName) : base(portName)
    {
        _serialPort = new SerialPort
        {
            PortName = portName,
            BaudRate = BAUD_RATE,
            ReadTimeout = 1000,
            WriteTimeout = 1000
        };
    }

    public override bool StartCapture()
    {
        try
        {
            _serialPort.Open();
            IsReady = true;
            return true;
        }
        catch (Exception ex)
        {
            BabbleCore.Instance.Logger.LogError($"Failed to open serial port {Url}: {ex.Message}");
            IsReady = false;
            return false;
        }
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
            BabbleCore.Instance.Logger.LogError($"Failed to close serial port {Url}: {ex.Message}");
            return false;
        }
    }


    private Mat GetNextFrame()
    {
        if (!IsReady || !_serialPort.IsOpen) return EmptyMat;

        try
        {
            if (_serialPort.BytesToRead > 0)
            {
                var (start, jpegSize) = GetNextPacketBounds();
                if (start == -1 || jpegSize == -1) return EmptyMat;

                byte[] jpegData = new byte[jpegSize];
                Array.Copy(_buffer, start + ETVR_HEADER_LEN, jpegData, 0, jpegSize);

                if (jpegData.Length >= 2 && jpegData[0] == 0xFF && jpegData[1] == 0xD8)
                {
                    _bufferPosition = 0;
                    CvInvoke.Imdecode(jpegData, ImreadModes.Color, RawFrame);
                    return RawFrame;
                }

                Array.Clear(_buffer, 0, _buffer.Length);
                _bufferPosition = 0;
                //Clear _buffer after each frame is processed by using Array.Clear(_buffer, 0, _buffer.Length); to prevent leftover data from impacting the next read.
            }
        }
        catch (Exception ex)
        {
            IsReady = false;
            _serialPort.Close();
            BabbleCore.Instance.Logger.LogError($"Error reading frame on port {Url} at buffer position {_bufferPosition}: {ex.Message}");
        }
        //Log more context about the frame data, such as current buffer position, byte count, and port status.


        return EmptyMat;
    }


    private (int start, int size) GetNextPacketBounds()
    {
        int headerPos = -1;
        byte[] header = ETVR_HEADER.Concat(ETVR_HEADER_FRAME).ToArray();

        // Keep reading until we find a valid header
        while (headerPos == -1)
        {
            // Read more data into our buffer
            int bytesRead = _serialPort.Read(_buffer, _bufferPosition,
                Math.Min(2048, _buffer.Length - _bufferPosition));

            if (bytesRead == 0)
            {
                return (-1, -1);
            }

            _bufferPosition += bytesRead;

            // Search for the protocol header
            for (int i = 0; i <= _bufferPosition - header.Length; i++)
            {
                if (CompareBytes(_buffer, i, header))
                {
                    headerPos = i;
                    break;
                }
            }
        }

        // Extract the JPEG data size from the header
        // The size is stored in 2 bytes after the 4-byte protocol header
        int jpegSize = BitConverter.ToUInt16(_buffer, headerPos + 4);

        // Ensure we have enough buffer space for the complete JPEG
        if (_buffer.Length < headerPos + ETVR_HEADER_LEN + jpegSize)
        {
            Array.Resize(ref _buffer, headerPos + ETVR_HEADER_LEN + jpegSize);
        }

        // Read any remaining data needed for the complete JPEG
        while (_bufferPosition < headerPos + ETVR_HEADER_LEN + jpegSize)
        {
            int bytesRead = _serialPort.Read(_buffer, _bufferPosition,
                Math.Min(2048, headerPos + ETVR_HEADER_LEN + jpegSize - _bufferPosition));
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
                StopCapture();
                _serialPort?.Dispose();
            }
            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SerialCamera()
    {
        Dispose(false);
    }
}
