using Babble.Core;
using Babble.Core.Settings;
using Rug.Osc;
using System.Diagnostics.CodeAnalysis;
using System.Net;
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
    private readonly OSCQuery _query;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private static readonly Parameter[] AllParams = [.. UnifiedTracking.AllParameters_v2, .. UnifiedTracking.AllParameters_v1];
    private readonly List<Parameter> CurrentAvatarParams = [];

    private static readonly string[] prefixes = ["", "FT/",];

    private const string DEFAULT_HOST = "127.0.0.1";

    private const int DEFAULT_LOCAL_PORT = 44444;

    private const int DEFAULT_REMOTE_PORT = 8888;

    private const int TIMEOUT_MS = 10000;

    public BabbleOSC(string? host = null, int? remotePort = null)
    {
        var settings = BabbleCore.Instance.Settings;
        
        _query = new OSCQuery(IPAddress.Loopback);
        _query.OnAvatarChange += DetermineNewParameters;

        OnBabbleSettingsChanged(settings);

        var ip = IPAddress.Parse(host ?? DEFAULT_HOST);
        ConfigureReceiver(
            ip, 
            remotePort ?? DEFAULT_REMOTE_PORT);

        _cancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(() => SendLoopAsync(_cancellationTokenSource.Token));
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
        if (_sender is not null)
        {
            if (_sender.State == OscSocketState.Connected) return;
            if (_sender.State == OscSocketState.Closing) return;
        }

        _sender = new OscSender(host, DEFAULT_LOCAL_PORT, remotePort)
        {
            DisconnectTimeout = TIMEOUT_MS
        };

        // This will throw an exception if a user takes their headset off if OSCQuery is in use
        _sender.Connect(); 
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!BabbleCore.Instance.IsRunning)
                continue;

            try
            {
                if (BabbleCore.Instance.Settings.GeneralSettings.GuiForceRelevancy)
                    await SendMobileParameters();
                else
                    await SendDesktopParameters();

                await PollConnectionStatus(cancellationToken);
                await Task.Delay(25, cancellationToken);
            }
            catch { }
        }
    }

    private Task SendMobileParameters()
    {
        var mul = BabbleCore.Instance.Settings.GeneralSettings.GuiMultiply;

        foreach (var prefix in prefixes)
        {
            foreach (var param in CurrentAvatarParams)
            {
                foreach (var (paramName, paramLiteral) in param.GetParamNames())
                {
                    if (paramName == "EyeTrackingActive")
                        continue;

                    switch (paramLiteral)
                    {
                        case BaseParam<float> floatName:
                            if (!floatName.Relevant) continue;
                            var address = $"/avatar/parameters/{prefix}{paramName}";
                            if (address.EndsWith("v2/")) continue; // Not a valid OSC address
                            float floatValue;
                            try
                            {
                                floatValue = floatName.ParamValue;
                            }
                            catch { continue; }
                            if (float.IsNaN(floatValue) || floatValue == 0) continue;
                            _sender.Send(new OscMessage(address, floatValue * (float)mul));
                            break;
                        // This only returns a single bool without Binary steps
                        case BaseParam<bool> boolName:
                            if (!boolName.Relevant) continue;
                            bool boolValue;
                            try
                            {
                                boolValue = boolName.ParamValue;
                            }
                            catch { continue; }
                            _sender.Send(new OscMessage($"/avatar/parameters/{prefix}{paramName}", boolValue));
                            break;
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private Task SendDesktopParameters()
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

        return Task.CompletedTask;
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

        CurrentAvatarParams.Clear();

        // Walk the contents tree, and for each parameter determine if it is a VRCFT parameter
        ParseOSCQueryNodeTree(avatarConfig);
    }

    private void ParseOSCQueryNodeTree(OscQueryNode avatarConfig)
    {
        var avatarInfo = new OscQueryAvatarInfo(avatarConfig);
        foreach (var parameter in AllParams)
            CurrentAvatarParams.AddRange(parameter.ResetParam(avatarInfo.Parameters));

        if (avatarConfig.Contents is not null)
        {
            foreach (var child in avatarConfig.Contents)
            {
                ParseOSCQueryNodeTree(child.Value);
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