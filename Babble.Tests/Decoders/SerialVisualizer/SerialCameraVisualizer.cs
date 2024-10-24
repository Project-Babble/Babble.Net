using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO.Ports;

namespace Babble.Maui.Scripts.Decoders;

public class SerialCameraVisualizer : Capture, IDisposable
{
    private const int BAUD_RATE = 3000000;
    private const int WAIT_TIME_MS = 100;
    private static readonly byte[] ETVR_HEADER = new byte[] { 0xff, 0xa0 };
    private static readonly byte[] ETVR_HEADER_FRAME = new byte[] { 0xff, 0xa1 };
    private const int ETVR_HEADER_LEN = 6;
    private const int BUFFER_SIZE = 2048;

    private readonly SerialPort _serialPort;
    private byte[] _buffer;
    private int _bufferPosition;
    private bool _isDisposed;
    private byte[] _currentFrame;
    private Task _captureTask;
    private CancellationTokenSource _cancellationTokenSource;

    public override string Url { get; set; }
    public override byte[] Frame => _currentFrame;
    public override bool IsReady { get; set; }

    public SerialCameraVisualizer(string portName) : base(portName)
    {
        if (!portName.ToLower().StartsWith("com"))
            throw new ArgumentException("SerialCameraCaptureTests must accept a COM port as input!");

        _buffer = new byte[BUFFER_SIZE];
        _bufferPosition = 0;

        _serialPort = new SerialPort
        {
            PortName = portName,
            BaudRate = BAUD_RATE,
            ReadTimeout = 1000,
            WriteTimeout = 1000,
            Handshake = Handshake.None
        };
    }

    public override bool StartCapture()
    {
        try
        {
            _serialPort.Open();
            Thread.Sleep(WAIT_TIME_MS);
            IsReady = true;

            _cancellationTokenSource = new CancellationTokenSource();
            _captureTask = Task.Run(CaptureLoop, _cancellationTokenSource.Token);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start capture: {ex.Message}");
            IsReady = false;
            return false;
        }
    }

    public override bool StopCapture()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _captureTask?.Wait();
            _serialPort?.Close();
            IsReady = false;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to stop capture: {ex.Message}");
            return false;
        }
    }

    private async Task CaptureLoop()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var (success, frame) = GetSerialCameraPicture();
                if (success && frame != null)
                {
                    // Convert Mat to byte array for Frame property
                    var img = frame.ToImage<Bgr, byte>();
                    _currentFrame = img.ToJpegData();
                    CvInvoke.Imshow("Serial Camera", img);
                    CvInvoke.WaitKey();
                }
                await Task.Delay(10); // Small delay to prevent CPU overuse
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Capture loop error: {ex.Message}");
                IsReady = false;
                break;
            }
        }
    }
    private (int start, int end) GetNextPacketBounds()
    {
        int headerPosition = -1;
        byte[] combinedHeader = ETVR_HEADER.Concat(ETVR_HEADER_FRAME).ToArray();

        while (headerPosition == -1)
        {
            // Read more data if buffer is getting full
            if (_bufferPosition >= _buffer.Length - BUFFER_SIZE)
            {
                byte[] newBuffer = new byte[_buffer.Length + BUFFER_SIZE];
                Array.Copy(_buffer, newBuffer, _buffer.Length);
                _buffer = newBuffer;
            }

            int bytesRead = _serialPort.Read(_buffer, _bufferPosition, BUFFER_SIZE);
            _bufferPosition += bytesRead;

            // Search for header in current buffer
            for (int i = 0; i <= _bufferPosition - combinedHeader.Length; i++)
            {
                if (CompareArraySegments(_buffer, i, combinedHeader, 0, combinedHeader.Length))
                {
                    headerPosition = i;
                    break;
                }
            }
        }

        // Discard data before header
        if (headerPosition > 0)
        {
            Array.Copy(_buffer, headerPosition, _buffer, 0, _bufferPosition - headerPosition);
            _bufferPosition -= headerPosition;
            headerPosition = 0;
        }

        // Read packet size from header
        if (_bufferPosition < ETVR_HEADER_LEN)
        {
            int needed = ETVR_HEADER_LEN - _bufferPosition;
            _serialPort.Read(_buffer, _bufferPosition, needed);
            _bufferPosition += needed;
        }

        int packetSize = BitConverter.ToUInt16(_buffer, 4);

        // Ensure we have the full packet
        while (_bufferPosition < packetSize)
        {
            if (_bufferPosition >= _buffer.Length)
            {
                Array.Resize(ref _buffer, _buffer.Length + BUFFER_SIZE);
            }

            int bytesRead = _serialPort.Read(_buffer, _bufferPosition,
                Math.Min(BUFFER_SIZE, packetSize - _bufferPosition));
            _bufferPosition += bytesRead;
        }

        return (headerPosition, packetSize);
    }

    private byte[] GetNextJpegFrame()
    {
        var (start, packetSize) = GetNextPacketBounds();

        byte[] jpeg = new byte[packetSize - ETVR_HEADER_LEN];
        Array.Copy(_buffer, start + ETVR_HEADER_LEN, jpeg, 0, jpeg.Length);

        // Move remaining data to start of buffer
        int remaining = _bufferPosition - (packetSize + ETVR_HEADER_LEN);
        if (remaining > 0)
        {
            Array.Copy(_buffer, packetSize + ETVR_HEADER_LEN, _buffer, 0, remaining);
        }
        _bufferPosition = remaining;

        return jpeg;
    }

    public (bool success, Mat image) GetSerialCameraPicture()
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            return (false, null);

        try
        {
            if (_serialPort.BytesToRead > 0)
            {
                byte[] jpeg = GetNextJpegFrame();
                if (jpeg != null && jpeg.Length > 0)
                {
                    Mat image = new Mat();
                    CvInvoke.Imdecode(jpeg, ImreadModes.Color, image);

                    if (image.IsEmpty)
                    {
                        Console.WriteLine("[WARN] Frame drop. Corrupted JPEG.");
                        return (true, null);
                    }

                    // Clear buffer if too much data has accumulated
                    if (_serialPort.BytesToRead >= 8192)
                    {
                        Console.WriteLine($"[INFO] Discarding the serial buffer ({_serialPort.BytesToRead} bytes)");
                        _serialPort.DiscardInBuffer();
                        _buffer = new byte[BUFFER_SIZE];
                        _bufferPosition = 0;
                    }

                    return (true, image);
                }
            }
            return (false, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Serial capture source problem: {ex.Message}");
            Console.WriteLine("[WARN] Assuming camera disconnected, waiting for reconnect.");
            _serialPort.Close();
            return (false, null);
        }
    }

    private bool CompareArraySegments(byte[] arr1, int start1, byte[] arr2, int start2, int length)
    {
        for (int i = 0; i < length; i++)
        {
            if (arr1[start1 + i] != arr2[start2 + i])
                return false;
        }
        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                StopCapture();
                _serialPort?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
            _isDisposed = true;
        }
    }
}