using Babble.Avalonia.Scripts;
using Babble.Avalonia.OSC;
using Babble.Core;
using Hypernex.ExtendedTracking;
using Microsoft.Extensions.Logging;
using Rug.Osc;
using System.Net;
using VRCFaceTracking;
using VRCFaceTracking.BabbleNative;

namespace Babble.OSC;

public class BabbleOSC
{
    private OscSender _sender;
    private ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _sendTask;

    private readonly int _resolvedLocalPort;

    private readonly int _resolvedRemotePort;

    private readonly string _resolvedHost;

    private int _connectionAttempts;

    public const string DEFAULT_HOST = "127.0.0.1";

    public const int DEFAULT_LOCAL_PORT = 44444;

    public const int DEFAULT_REMOTE_PORT = 8888;

    private const int TIMEOUT_MS = 10000;

    // VRC has a 100ms limit when sending a "cluster" of OSC messages, or 10 messages per second
    // So we'll send 8 (clusters of) messages per second!
    private const int SEND_INTERVAL_MS_FLOOR = 125; 

    private const int MAX_RETRIES = 5;

    public event EventHandler? OnConnectionLost;

    public BabbleOSC(string? host = null, int? remotePort = null)
    {
        _resolvedHost = host ?? DEFAULT_HOST;
        _resolvedRemotePort = remotePort ?? DEFAULT_REMOTE_PORT;
        _resolvedLocalPort = DEFAULT_LOCAL_PORT;

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = factory.CreateLogger("BabbleOSC");

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

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        var generalSettings = BabbleCore.Instance.Settings.GeneralSettings;
        var allParams = VRCFTParameters.GetParameters();
        int[] binaryPowers = [1, 2, 4, 8];

        while (!cancellationToken.IsCancellationRequested)
        {  
            if (!BabbleCore.Instance.IsRunning)
            {
                await Task.Delay(SEND_INTERVAL_MS_FLOOR, cancellationToken);
                continue;
            }

            var forceRelevancy = generalSettings.GuiForceRelevancy;
            var prefix = generalSettings.GuiOscLocation;

            try
            {
                if (forceRelevancy && _sender.State == OscSocketState.Connected)
                {
                    _connectionAttempts = 0;
                    foreach (var param in allParams)
                    {
                        var value = param.GetWeight(UnifiedTracking.Data);
                        if (value == 0 || float.IsNaN(value)) 
                            continue;

                        var trimmed = param.Name.TrimEnd('/');
                        _sender.Send(new OscMessage($"{prefix}{trimmed}", value));
                        _sender.Send(new OscMessage($"{prefix}v2/{trimmed}", value));
                        // _sender.Send(new OscMessage($"{prefix}{trimmed}Negative", -value));
                        // _sender.Send(new OscMessage($"{prefix}v2/{trimmed}Negative", -value));


                        //var bitsWithPowers = Float8Converter.GetBits(param.GetWeight(UnifiedTracking.Data));
                        //for (int i = 0; i < binaryPowers.Length; i++)
                        //{
                        //    _sender.Send(new OscMessage($"{prefix}{trimmed}{binaryPowers[i]}", bitsWithPowers[i]));
                        //    _sender.Send(new OscMessage($"{prefix}v2/{trimmed}{binaryPowers[i]}", bitsWithPowers[i]));
                        //}
                    }

                    // If sending directly to VRChat, make sure we don't spam the queue
                    await Task.Delay(SEND_INTERVAL_MS_FLOOR, cancellationToken);
                }
                else if (_sender.State == OscSocketState.Connected)
                {
                    _connectionAttempts = 0;
                    var mul = (float)generalSettings.GuiMultiply;
                    foreach (var exp in BabbleMapping.Mapping)
                    {
                        var address = BabbleAddresses.Addresses[exp.Key];
                        var value = UnifiedTracking.Data.Shapes[(int) exp.Value].Weight;
                        if (value == 0)
                            continue;

                        _sender.Send(new OscMessage($"{prefix}{address}", value * mul));
                    }
                }
                else if (_sender.State == OscSocketState.Closed)
                {
                    if (_connectionAttempts < MAX_RETRIES)
                    {
                        _connectionAttempts++;

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
                }
            }
            catch (Exception)
            {
                // If there's an error here, don't freeze the UI thread
                await Task.Delay(SEND_INTERVAL_MS_FLOOR, cancellationToken);
            }
            finally
            {
                // Don't max out CPU
                await Task.Delay(10, cancellationToken);
            }
        }
    }
    
    public void Teardown()
    {
        _cancellationTokenSource.Cancel();
        _sender.Close();
        _sender.Dispose();
        _cancellationTokenSource.Dispose();
    }
}