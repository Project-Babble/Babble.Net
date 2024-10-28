using Babble.Core.Scripts;
using Rug.Osc;
using System.Net;

namespace Babble.OSC;

public partial class BabbleOSC
{
    private OscSender _sender;

    private bool _loop = true;

    private readonly Thread _thread;

    private readonly int _resolvedLocalPort;

    private readonly int _resolvedRemotePort;

    private readonly string _resolvedHost;

    public const string DEFAULT_HOST = "127.0.0.1";

    public const int DEFAULT_LOCAL_PORT = 44444;

    public const int DEFAULT_REMOTE_PORT = 8888;

    private const int TIMEOUT_MS = 10000;


    public BabbleOSC(string? host = null, int? remotePort = null, int ? localPort = null)
    {
        _resolvedHost = host ?? DEFAULT_HOST;
        _resolvedLocalPort = localPort ?? DEFAULT_LOCAL_PORT;
        _resolvedRemotePort = remotePort ?? DEFAULT_REMOTE_PORT;

        ConfigureReceiver();

        _loop = true;
        _thread = new Thread(new ThreadStart(SendLoop));
        _thread.Start();
    }


    private void ConfigureReceiver()
    {
        IPAddress address = IPAddress.Parse(_resolvedHost);
        _sender = new OscSender(address, _resolvedLocalPort, _resolvedRemotePort)
        {
            DisconnectTimeout = TIMEOUT_MS
        };
        _sender.Connect();
    }

    private void SendLoop()
    {
        while (_loop)
        {
            try
            {
                switch (_sender.State)
                {
                    case OscSocketState.Connected:
                        foreach (var exp in Expressions.InnerKeys)
                            _sender.Send(new OscMessage(exp, Expressions.GetByKey2(exp)));
                        break;
                    case OscSocketState.Closed:
                        _sender.Close();
                        _sender.Dispose();
                        ConfigureReceiver();
                        break;
                }
            }
            catch (Exception)
            {
                // Ignore network exceptions
            }

            Thread.Sleep(Utils.THREAD_TIMEOUT_MS);
        }
    }

    public void Teardown()
    {
        _loop = false;
        _sender.Close();
        _sender.Dispose();
        _thread.Join();
    }
}