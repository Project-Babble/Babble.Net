using System.Net;

// https://gist.github.com/lightfromshadows/79029ca480393270009173abc7cad858
public class MJPEGStreamDecoder : IDisposable
{
    public byte[] Frame { get; private set; }

    private const int MAX_RETRIES = 3;
    private int _retryCount = 0; 
    private Thread _worker;
    private List<BufferedStream> _trackedBuffers = new List<BufferedStream>();

    public void StartStream(string url)
    {
        _retryCount = 0;
        Dispose();

        _worker = new Thread(() => ReadMJPEGStreamWorker(url));
        _worker.Start();
    }

    private void ReadMJPEGStreamWorker(string url)
    {
        var webRequest = WebRequest.Create(url);
        webRequest.Method = "GET";
        var frameBuffer = new List<byte>();

        int lastByte = 0x00;
        bool addToBuffer = false;

        BufferedStream buffer = null;
        try
        {
            Stream stream = webRequest.GetResponse().GetResponseStream();
            buffer = new BufferedStream(stream);
            _trackedBuffers.Add(buffer);
        }
        catch (Exception ex)
        {

        }

        int newByte;
        while (buffer != null)
        {
            if (!buffer.CanRead)
            {
                break;
            }

            newByte = -1;

            try
            {
                newByte = buffer.ReadByte();
            }
            catch
            {
                break; // Something happened to the stream, start a new one
            }

            if (newByte < 0) // End of stream or failure
            {
                continue; // End of data
            }

            if (addToBuffer)
            {
                frameBuffer.Add((byte)newByte);
            }

            if (lastByte == 0xFF) // It's a command!
            {
                if (!addToBuffer) // We're not reading a frame, should we be?
                {
                    if (IsStartOfImage(newByte))
                    {
                        addToBuffer = true;
                        frameBuffer.Add((byte)lastByte);
                        frameBuffer.Add((byte)newByte);
                    }
                }
                else // We're reading a frame, should we stop?
                {
                    if (newByte == 0xD9)
                    {
                        frameBuffer.Add((byte)newByte);
                        addToBuffer = false;
                        Frame = frameBuffer.ToArray();
                        frameBuffer.Clear();
                    }
                }
            }

            lastByte = newByte;
        }

        if (_retryCount < MAX_RETRIES)
        {
            _retryCount++;

            Dispose();
            _trackedBuffers.Clear();

            _worker = new Thread(() => ReadMJPEGStreamWorker(url));
            _worker.Start();
        }
    }

    private bool IsStartOfImage(int command)
    {
        switch (command)
        {
            case 0x8D:
            case 0xC0:
            case 0xC2:
            case 0xD8:
                return true;
            //case 0xC4:
            //case 0xDD:
            //case 0xDA:
            //case 0xFE:
            //case 0xD9:
            //    break;
        }
        return false;
    }

    public void Dispose()
    {
        foreach (var b in _trackedBuffers)
        {
            if (b != null)
                b.Close();
        }
    }
}
