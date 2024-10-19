using System.Net;
using System.Net.Sockets;

namespace Babble.OSC;

public class BabbleOSC
{
    private Socket _receiver;

    private bool _loop = true;

    private readonly Thread _thread;

    private readonly int _resolvedPort;

    private readonly string _resolvedHost;

    public const string DEFAULT_HOST = "127.0.0.1";

    public const int DEFAULT_PORT = 8888;

    private const int TIMEOUT_MS = 10000;

    public BabbleOSC(string? host = null, int? port = null)
    {
        _resolvedHost = host ?? DEFAULT_HOST;
        _resolvedPort = port ?? TIMEOUT_MS;

        ConfigureReceiver();
        _loop = true;
        _thread = new Thread(new ThreadStart(SendLoop));
        _thread.Start();
    }

    private void ConfigureReceiver()
    {
        IPAddress address = IPAddress.Parse(_resolvedHost);
        IPEndPoint localEP = new IPEndPoint(address, _resolvedPort);
        _receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _receiver.Bind(localEP);
        _receiver.ReceiveTimeout = TIMEOUT_MS;
    }

    private void SendLoop()
    {
        while (_loop)
        {
            try
            {
                if (_receiver.IsBound)
                {
                    // TODO Fill this out
                    _receiver.Send();
                }
                else
                {
                    _receiver.Close();
                    _receiver.Dispose();
                    ConfigureReceiver();
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public void Teardown()
    {
        _loop = false;
        _receiver.Close();
        _receiver.Dispose();
        _thread.Join();
    }
}