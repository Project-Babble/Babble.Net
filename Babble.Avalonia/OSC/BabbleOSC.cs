using Babble.Core;
using Babble.Core.Settings;
using Rug.Osc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;
using VRCFaceTracking;
using VRCFaceTracking.BabbleNative;
using VRCFaceTracking.Core.OSC.DataTypes;
using VRCFaceTracking.Core.OSC.Query;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFTReceiver;

namespace Babble.OSC;

public class BabbleOSC
{
    private OscSender _sender;
    private OSCQuery _query;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _sendTask;
    private static readonly Parameter[] _allParams = UnifiedTracking.AllParameters_v2.Concat(UnifiedTracking.AllParameters_v1).ToArray();
    private readonly List<Parameter> _currentParams = [];

    private static readonly string[] prefixes = ["", "FT/",];

    private const string DEFAULT_HOST = "127.0.0.1";

    private const int DEFAULT_LOCAL_PORT = 44444;

    private const int DEFAULT_REMOTE_PORT = 8888;

    private const int TIMEOUT_MS = 10000;

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
       
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!BabbleCore.Instance.IsRunning)
            {
                continue;
            }

            if (_sender.State != OscSocketState.Connected)
            {
                return;
            }

            try
            {
                if (BabbleCore.Instance.Settings.GeneralSettings.GuiForceRelevancy)
                    await SendMobileParameters(cancellationToken);
                else
                    await SendDesktopParameters(cancellationToken);

                await PollConnectionStatus(cancellationToken);
            }
            catch (Exception)
            {
                // If there's an error here, don't freeze the UI thread
            }
        }
    }

    private async Task SendMobileParameters(CancellationToken cancellationToken)
    {
        var mul = BabbleCore.Instance.Settings.GeneralSettings.GuiMultiply;

        foreach (var prefix in prefixes)
        {
            foreach (var param in _currentParams)
            {
                foreach (var name in param.GetParamNames())
                {
                    switch (name.paramLiteral)
                    {
                        case BaseParam<float> floatName:
                            if (floatName.Relevant)
                            {
                                try
                                {
                                    var address = $"/avatar/parameters/{prefix}{name.paramName}";
                                    if (address.EndsWith("v2/"))
                                        continue; // Not a valid OSC address

                                    _sender.Send(new OscMessage(address, floatName.ParamValue * mul));
                                }
                                catch (Exception e)
                                {
                                    // Handle weird null ref exceptions on OSCMessage
                                }
                            }
                            break;
                        //case BaseParam<bool> boolName:
                        //    if (boolName.Relevant)
                        //    {
                        //        _sender.Send(new OscMessage($"/avatar/parameters/{prefix}{name.paramName}", boolName.ParamValue));
                        //    }
                        //    break;
                    }
                }
            }
        }
    }

    private async Task SendDesktopParameters(CancellationToken cancellationToken)
    {
        var mul = BabbleCore.Instance.Settings.GeneralSettings.GuiMultiply;
        var prefix = BabbleCore.Instance.Settings.GeneralSettings.GuiOscLocation;

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

            _sender.Send(new OscMessage($"{prefix}{address}", value * (float)mul));
        }
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

    private void DetermineNewParameters(OscQueryNode avatarConfig)
    {
        // Here, determine from OSCQuery what VRCFTProgrammableExpressions we need to update for this (new!) avatar
        // This method is intense, but only runs when a user changes their avatar so it should be OK

        _currentParams.Clear();

        var avatarInfo = new OscQueryAvatarInfo(avatarConfig);
        foreach (var parameter in _allParams)
        {
            _currentParams.AddRange(parameter.ResetParam(avatarInfo.Parameters));
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