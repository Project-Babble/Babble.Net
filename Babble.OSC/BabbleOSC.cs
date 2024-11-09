using Babble.Core;
using Rug.Osc;
using System.Net;

namespace Babble.OSC;

public partial class BabbleOSC
{
    private OscSender _sender;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _sendTask;

    private const int maxretries = 5;
    private int _connectionattemtps;
    private const int SEND_INTERVAL_MS = 10;
    private readonly int _resolvedLocalPort;

    private readonly int _resolvedRemotePort;

    private readonly string _resolvedHost;

    public const string DEFAULT_HOST = "127.0.0.1";

    public const int DEFAULT_LOCAL_PORT = 44444;

    public const int DEFAULT_REMOTE_PORT = 8888;

    private const int TIMEOUT_MS = 10000;
    public event EventHandler? OnConnectionLost;

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
                        _connectionattemtps = 0; // Reset retry counter on successful connection

                        foreach (var exp in Expressions.InnerKeys)
                        {
                            var message = new OscMessage($"/{prefix}{exp}", Expressions.GetByKey2(exp) * mul);
                            _sender.Send(message);  // Using OscSender.Send directly
                        }
                        break;

                    case OscSocketState.Closed:
                        if (_connectionattemtps < maxretries)
                        {
                            _connectionattemtps++;

                            // Close and dispose the current sender
                            _sender.Close();
                            _sender.Dispose();

                            // Delay before attempting to reconnect
                            await Task.Delay(1000, cancellationToken);

                            // Attempt to reconfigure the receiver and reconnect
                            ConfigureReceiver();
                        }
                        else
                        {
                            // Trigger the connection lost event after max retries reached
                            OnConnectionLost?.Invoke(this, EventArgs.Empty);
                            return; // Exit the loop
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                //IGNORE THIS
            }

            // Delay between each loop iteration to avoid high CPU usage
            await Task.Delay(SEND_INTERVAL_MS, cancellationToken);
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