using System.IO.Ports;

namespace Babble.Maui.Scripts.Decoders;

public class SerialCamera : Capture, IDisposable
{
    // Protocol constants
    private const int BAUD_RATE = 3000000;
    private static readonly byte[] ETVR_HEADER = new byte[] { 0xff, 0xa0 };
    private static readonly byte[] ETVR_HEADER_FRAME = new byte[] { 0xff, 0xa1 };
    private const int ETVR_HEADER_LEN = 6;  // 2 bytes header + 2 bytes frame type + 2 bytes size

    private readonly SerialPort _serialPort;
    private byte[] _buffer = new byte[2048];
    private int _bufferPosition;
    private bool _isDisposed;

    public override string Url { get; set; }
    public override byte[] Frame => GetNextFrame();
    public override bool IsReady { get; set; }

    public SerialCamera(string portName) : base(portName)
    {
        if (!portName.ToLower().StartsWith("com"))
            throw new ArgumentException("SerialCameraCaptureTests must accept a COM port as input!");

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
        catch
        {
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
        catch
        {
            return false;
        }
    }

    private byte[] GetNextFrame()
    {
        if (!IsReady || !_serialPort.IsOpen) return null;

        try
        {
            if (_serialPort.BytesToRead > 0)
            {
                // Get the packet boundaries which contain our JPEG data
                var (start, jpegSize) = GetNextPacketBounds();

                // Create a new array exactly sized for the JPEG data
                byte[] jpegData = new byte[jpegSize];

                // Copy just the JPEG portion (skipping protocol headers)
                Array.Copy(_buffer, start + ETVR_HEADER_LEN, jpegData, 0, jpegSize);

                // Verify JPEG header (0xFF 0xD8)
                if (jpegData.Length >= 2 && jpegData[0] == 0xFF && jpegData[1] == 0xD8)
                {
                    // Reset buffer position for next frame
                    _bufferPosition = 0;
                    return jpegData;
                }

                // If we didn't find valid JPEG data, reset and return null
                _bufferPosition = 0;
            }
        }
        catch
        {
            IsReady = false;
            _serialPort.Close();
        }

        return null;
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

    public void Dispose()
    {
        if (!_isDisposed)
        {
            StopCapture();
            _serialPort?.Dispose();
            _isDisposed = true;
        }
    }
}