using Babble.Avalonia.OSC;
using Babble.Core;
using Babble.Core.Settings;
using Babble.Core.Settings.Models;
using Hypernex.ExtendedTracking;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rug.Osc;
using System.Net;
using System.Text;
using VRCFaceTracking;
using VRCFaceTracking.BabbleNative;

namespace Babble.OSC;

public class BabbleOSC
{
    private OscSender _sender;
    private int _tcpPort;
    private ILogger _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _sendTask;

    private const string DEFAULT_HOST = "127.0.0.1";

    private const int DEFAULT_LOCAL_PORT = 44444;

    private const int DEFAULT_REMOTE_PORT = 8888;

    private const int TIMEOUT_MS = 10000;
    
    private const int SEND_INTERVAL_MS_MOBILE = 125;

    private const int SEND_INTERVAL_MS_DESKTOP = 10;

    private const int MAX_RETRIES = 5;

    public BabbleOSC(string? host = null, int? remotePort = null)
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = factory.CreateLogger("BabbleOSC");

        var settings = BabbleCore.Instance.Settings;
        var ip = IPAddress.Parse(host ?? DEFAULT_HOST);
        OnBabbleSettingsChanged(settings);

        ConfigureReceiver(
            ip, 
            remotePort ?? DEFAULT_REMOTE_PORT);

        _cancellationTokenSource = new CancellationTokenSource();
        _sendTask = Task.Run(() => SendLoopAsync(_cancellationTokenSource.Token));
    }

    private void OnBabbleSettingsChanged(BabbleSettings settings)
    {
        settings.OnUpdate += async (setting) =>
        {
            // Hacky but it works
            var normalizedSetting = setting.Replace("_", string.Empty).ToLower();
            if (normalizedSetting == "guioscaddress" || normalizedSetting == "guioscport")
            {
                // Close and dispose the current sender
                _sender.Close();
                _sender.Dispose();

                // Delay before attempting to reconnect
                await Task.Delay(1000);

                // Attempt to reconfigure the receiver and reconnect
                ConfigureReceiver(
                    IPAddress.Parse(settings.GeneralSettings.GuiOscAddress),
                    settings.GeneralSettings.GuiOscPort);
            }
        };
    }

    private void ConfigureReceiver(IPAddress host, int remotePort)
    {
        _sender = new OscSender(host, DEFAULT_LOCAL_PORT, remotePort)
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
                await Task.Delay(SEND_INTERVAL_MS_MOBILE, cancellationToken);
                continue;
            }

            var forceRelevancy = generalSettings.GuiForceRelevancy;
            var prefix = generalSettings.GuiOscLocation;

            try
            {
                await SendMobileParameters(allParams, forceRelevancy, prefix, cancellationToken);
                await SendDesktopParameters(generalSettings, prefix, cancellationToken);
                await PollConnectionStatus(cancellationToken);
            }
            catch (Exception)
            {
                // If there's an error here, don't freeze the UI thread
                await Task.Delay(SEND_INTERVAL_MS_MOBILE, cancellationToken);
            }
        }
    }

    private async Task SendMobileParameters(List<VRCFTParameters.VRCFTProgrammableExpression> allParams, bool forceRelevancy, string prefix, CancellationToken cancellationToken)
    {
        if (!forceRelevancy || _sender.State != OscSocketState.Connected)
        {
            return;
        }

        foreach (var param in allParams)
        {
            var value = param.GetWeight(UnifiedTracking.Data);
            if (value == 0 || float.IsNaN(value))
                continue;

            var trimmed = param.Name.TrimEnd('/');
            _sender.Send(new OscMessage($"{prefix}{trimmed}", value));

            // To be replaced with OSCQuery
            // _sender.Send(new OscMessage($"{prefix}v2/{trimmed}", value));
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
        await Task.Delay(SEND_INTERVAL_MS_MOBILE, cancellationToken);
    }

    private async Task SendDesktopParameters(GeneralSettings generalSettings, string prefix, CancellationToken cancellationToken)
    {
        if (_sender.State != OscSocketState.Connected)
        {
            return;
        }

        var mul = (float)generalSettings.GuiMultiply;
        foreach (var exp in BabbleMapping.Mapping)
        {
            var address = BabbleAddresses.Addresses[exp.Key];
            var value = UnifiedTracking.Data.Shapes[(int)exp.Value].Weight;
            if (value == 0)
                continue;

            _sender.Send(new OscMessage($"{prefix}{address}", value * mul));
        }

        await Task.Delay(SEND_INTERVAL_MS_DESKTOP, cancellationToken);
    }

    private async Task PollConnectionStatus(CancellationToken cancellationToken)
    {
        if (_sender.State != OscSocketState.Closed)
        {
            return;
        }

        // Close and dispose the current sender
        _sender.Close();
        _sender.Dispose();

        // Delay before attempting to reconnect
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        // Attempt to reconfigure the receiver and reconnect
        ConfigureReceiver(_sender.RemoteAddress, _sender.Port);
    }

    public void Teardown()
    {
        _cancellationTokenSource.Cancel();
        _sender.Close();
        _sender.Dispose();
        _cancellationTokenSource.Dispose();
    }
}