using System.IO.Ports;

namespace Babble.Maui.Scripts.Decoders;

/// <summary>
/// COM
/// </summary>
internal class SerialCameraCapture : BytesCapture
{
    public override bool IsReady { get; set; }
    public override byte[] Frame { get; }

    private const int BaudRate = 3000000;
    private const int BufferSize = 32768;
    private const string ETVR_HEADER = "\xff\xa0";
    private const string ETVR_HEADER_FRAME = "\xff\xa1";
    private const int ETVR_HEADER_LEN = 6;

    private SerialPort _serialPort;
    private byte[] _buffer = new byte[BufferSize];

    public SerialCameraCapture(string Url) : base(Url)
    {
    }

    public override bool StartCapture()
    {
        // Initialize serial port
        _serialPort = new SerialPort(Url, BaudRate, Parity.None, 8, StopBits.One);
        _serialPort.Open();
        _serialPort.ReadTimeout = 500;
        _serialPort.DataReceived += SerialPort_DataReceived;
        IsReady = true;

        return true;
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        int bytesToRead = _serialPort.BytesToRead;
        _serialPort.Read(_buffer, 0, bytesToRead);

        // Process buffer and extract JPEG frame
        Array.Copy(ExtractJpegFrame(_buffer, bytesToRead), Frame, Frame.Length);
    }

    private byte[] ExtractJpegFrame(byte[] buffer, int bytesToRead)
    {
        int beg = Array.IndexOf(buffer, Convert.ToByte(ETVR_HEADER_FRAME));
        if (beg == -1)
            return Array.Empty<byte>();

        int packetSize = BitConverter.ToInt16(buffer, beg + 4);
        if (packetSize > bytesToRead)
            return Array.Empty<byte>();

        byte[] jpegFrame = new byte[packetSize];
        Array.Copy(buffer, beg + ETVR_HEADER_LEN, jpegFrame, 0, packetSize);

        return jpegFrame;
    }

    public override bool StopCapture()
    {
        IsReady = false;
        _serialPort.Close();
        return true;
    }
}
