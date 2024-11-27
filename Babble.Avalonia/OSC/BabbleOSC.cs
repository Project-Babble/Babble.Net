using Babble.Avalonia.Scripts;
using Babble.Core;
using Babble.Core.Settings;
using Babble.Core.Settings.Models;
using Rug.Osc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;
using VRCFaceTracking;
using VRCFaceTracking.BabbleNative;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFTReceiver;
using static Hypernex.ExtendedTracking.VRCFTParameters;

namespace Babble.OSC;

public class BabbleOSC
{
    private OscSender _sender;
    private OSCQuery _query;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _sendTask;
    private readonly List<VRCFTProgrammableExpression> _allParams = GetParameters();
    private readonly HashSet<VRCFTProgrammableExpression> _currentFloatParams = [];
    private readonly HashSet<VRCFTProgrammableExpression> _currentBinaryParams = [];

    private const string DEFAULT_HOST = "127.0.0.1";

    private const int DEFAULT_LOCAL_PORT = 44444;

    private const int DEFAULT_REMOTE_PORT = 8888;

    private const int TIMEOUT_MS = 10000;
    
    private const int SEND_INTERVAL_MS_MOBILE = 125;

    private const int SEND_INTERVAL_MS_DESKTOP = 10;

    private const int MAX_RETRIES = 5;

    public BabbleOSC(string? host = null, int? remotePort = null)
    {
        var settings = BabbleCore.Instance.Settings;
        var ip = IPAddress.Parse(host ?? DEFAULT_HOST);
        _query = new OSCQuery(ip, BabbleCore.Instance.Settings.GeneralSettings.GuiOscReceiverPort); // 9001 by default
        _query.OnAvatarChange += DetermineNewParameters;

        OnBabbleSettingsChanged(settings);

        ConfigureReceiver(
            ip, 
            remotePort ?? DEFAULT_REMOTE_PORT);

        _cancellationTokenSource = new CancellationTokenSource();
        _sendTask = Task.Run(() => SendLoopAsync(_cancellationTokenSource.Token));
    }

    private void OnBabbleSettingsChanged(BabbleSettings settings)
    {
        var guiOSCAddress = nameof(settings.GeneralSettings.GuiOscAddress);
        var guiOSCPort= nameof(settings.GeneralSettings.GuiOscPort);

        settings.OnUpdate += async (setting) =>
        {
            if (setting == guiOSCAddress || setting == guiOSCPort)
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

    [MemberNotNull(nameof(_sender))]
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
        int[] binaryPowers = [1, 2, 4, 8];

        while (!cancellationToken.IsCancellationRequested)
        {  
            if (!BabbleCore.Instance.IsRunning)
            {
                await Task.Delay(SEND_INTERVAL_MS_MOBILE, cancellationToken);
                continue;
            }

            var useOSCQuery = generalSettings.GuiForceRelevancy;
            var prefix = generalSettings.GuiOscLocation;

            try
            {
                //if (useOSCQuery)
                //    await SendMobileParameters(cancellationToken);
                //else
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

    private async Task SendMobileParameters(CancellationToken cancellationToken)
    {
        if (_sender.State != OscSocketState.Connected)
        {
            return;
        }

        foreach (var param in _currentFloatParams)
        {
            float value = param.GetWeight(UnifiedTracking.Data);
            if (value == 0 || float.IsNaN(value))
                continue;

            _sender.Send(new OscMessage($"/avatar/parameters/{param.Name}", value));
        }

        foreach (var param in _currentBinaryParams)
        {
            bool[] bitsWithPowers = Float8Converter.GetBits(param.GetWeight(UnifiedTracking.Data));
            _sender.Send(new OscMessage($"/avatar/parameters/{param.Name}Negative", bitsWithPowers[0])); // 0 bit is negative

            for (int i = 0; i < 3; i++) // Hardcoded power level (Max resolution of 8 here)
            {
                var power = (int)Math.Pow(2, i);
                _sender.Send(new OscMessage($"/avatar/parameters/{param.Name}{power}", bitsWithPowers[bitsWithPowers.Length - 1 - i]));
            }
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
            // Don't send the UE copies of pucker/funnel
            var address = BabbleAddresses.Addresses[exp.Key];
            var value = UnifiedTracking.Data.Shapes[(int)exp.Value].Weight;
            if (value == 0 ||
                exp.Value == UnifiedExpressions.LipFunnelLowerRight ||
                exp.Value == UnifiedExpressions.LipFunnelUpperLeft ||
                exp.Value == UnifiedExpressions.LipFunnelUpperRight ||
                exp.Value == UnifiedExpressions.LipPuckerLowerRight ||
                exp.Value == UnifiedExpressions.LipPuckerUpperLeft ||
                exp.Value == UnifiedExpressions.LipPuckerUpperRight)
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

    private void DetermineNewParameters(HashSet<string> set)
    {
        // Here, determine from OSCQuery what VRCFTProgrammableExpressions we need to update for this (new!) avatar
        // This method is intense, but only runs when a user changes their avatar so it should be OK

        _currentFloatParams.Clear();
        _currentBinaryParams.Clear();

        foreach (var param in _allParams)
        {
            foreach (var item in set)
            {
                // For floats
                if (param.IsMatch(item))
                {
                    _currentFloatParams.Add(param);
                    break;
                }

                // For binary parameters, we need to do some extract work. We'll ask it some questions:

                // 1) Does this parameter, with any and all numbers from the end removed, constitute a possible parameter?                
                (string strippedString, int extractedNumber) pairing = StripTrailingNumbers(item);
                if (param.IsMatch(pairing.strippedString))
                {
                    // Or is this our first time seeing this param?
                    if (_currentBinaryParams.Add(param))
                    {
                        // If it is, fly
                        break;
                    }
                }
            }
        }
    }

    private static (string strippedString, int extractedNumber) StripTrailingNumbers(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (input, 0);

        // Regex to match trailing numbers
        var match = Regex.Match(input, @"(\d+)$", RegexOptions.Compiled);

        if (match.Success)
        {
            string numberPart = match.Value;
            string strippedString = input.Substring(0, input.Length - numberPart.Length);
            return (strippedString, int.Parse(numberPart));
        }

        return (input, 0);
    }

    public void Teardown()
    {
        _cancellationTokenSource.Cancel();
        _sender.Close();
        _sender.Dispose();
        _cancellationTokenSource.Dispose();
    }
}