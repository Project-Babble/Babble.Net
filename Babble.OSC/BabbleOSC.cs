using Babble.Core;
using Rug.Osc;
using System.Net;

namespace Babble.OSC;

public partial class BabbleOSC
{
    private OscSender _sender;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _sendTask;



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

        _cancellationTokenSource  = new CancellationTokenSource();
        _sendTask = Task.Run(() => SendLoopAsync(_cancellationTokenSource.Token));
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

    //refactored to a async task for non blocking execution.
    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        var settings = BabbleCore.Instance.Settings;

        while (!cancellationToken.IsCancellationRequested)
        {
            var prefix = settings.GetSetting<string>("gui_osc_location");
            var mul = settings.GetSetting<double>("gui_multiply");
            
            try
            {
                switch (_sender.State)
                {
                    case OscSocketState.Connected:
                        foreach (var exp in Expressions.InnerKeys)
                            _sender.Send(new OscMessage($"/{prefix}{exp}", Expressions.GetByKey2(exp) * mul));
                        break;
                    case OscSocketState.Closed:
                        _sender.Close();
                        _sender.Dispose();
                        ConfigureReceiver();
                        break;
                }
            }
            catch (Exception e)
            {
                // Ignore network exceptions 
            }

            await Task.Delay(10, cancellationToken);

        }
    }


    //Cancels the token waits for _sendTask to finish and then disposes of all resources allocated.
    public void Teardown()
    {
        _cancellationTokenSource.Cancel();
        _sender.Close();
        _sender.Dispose();
        _cancellationTokenSource.Dispose();
    }
}