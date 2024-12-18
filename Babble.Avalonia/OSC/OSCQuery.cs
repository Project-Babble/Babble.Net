using Babble.Core;
using Babble.Core.Scripts;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using VRC.OSCQuery;

namespace VRCFTReceiver;

public class OSCQuery
{
    private HashSet<OSCQueryServiceProfile> _profiles = [];
    private OSCQueryService _service = null!;

    private static readonly Regex VRChatClientRegex = new(@"VRChat-Client-[A-Za-z0-9]{6}$", RegexOptions.Compiled);
    private CancellationTokenSource _cancellationTokenSource;
    private const int VRC_PORT = 9000;

    public OSCQuery(IPAddress hostIP)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var tcpPort = Extensions.GetAvailableTcpPort();
        var udpPort = Extensions.GetAvailableUdpPort();

        _service = new OSCQueryServiceBuilder()
          .WithDiscovery(new MeaModDiscovery())
          .WithHostIP(hostIP)
          .WithTcpPort(tcpPort)
          .WithUdpPort(udpPort)
          .WithServiceName($"VRChat-Client-BabbleApp-{Utils.RandomString()}") // Yes this has to start with "VRChat-Client" https://github.com/benaclejames/VRCFaceTracking/blob/f687b143037f8f1a37a3aabf97baa06309b500a1/VRCFaceTracking.Core/mDNS/MulticastDnsService.cs#L195
          .StartHttpServer()
          .AdvertiseOSCQuery()
          .AdvertiseOSC()
          .Build();

        BabbleCore.Instance.Logger.LogInformation($"[VRCFTReceiver] Started OSCQueryService {_service.ServerName} at TCP {tcpPort}, UDP {udpPort}, HTTP http://{_service.HostIP}:{tcpPort}");

        _service.AddEndpoint<string>("/avatar/change", Attributes.AccessValues.ReadWrite, ["default"]);

        _service.OnOscQueryServiceAdded += AddProfileToList;

        StartAutoRefreshServices(5000);
    }

    private void AddProfileToList(OSCQueryServiceProfile profile)
    {
        if (_profiles.Contains(profile) || profile.port == _service.TcpPort)
        {
            return;
        }
        _profiles.Add(profile);
        BabbleCore.Instance.Logger.LogInformation($"[VRCFTReceiver] Added {profile.name} to list of OSCQuery profiles, at address http://{profile.address}:{profile.port}");
    }

    private void StartAutoRefreshServices(double interval)
    {
        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery start StartAutoRefreshServices");
        Task.Run(async () =>
        {
            while (true)
            {
                if (BabbleCore.Instance.Settings.GeneralSettings.GuiForceRelevancy)
                {
                    try
                    {
                        _service.RefreshServices();
                        await PollVRChatParameters();
                    }
                    catch (Exception)
                    {
                        
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(interval));
            }
        });
    }

    private async Task PollVRChatParameters()
    {
        if (_profiles == null || _profiles.Count == 0) return;
        
        try
        {
            var vrcProfile = _profiles.First(profile => VRChatClientRegex.IsMatch(profile.name));
            if (vrcProfile == null) 
                return;

            var ip = vrcProfile.address.ToString();
            if (BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress != ip)
            {
                BabbleCore.Instance.Settings.UpdateSetting<string>(
                    nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress),
                    vrcProfile.address.ToString());
            }

            if (BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort != VRC_PORT)
            {
                BabbleCore.Instance.Settings.UpdateSetting<int>(
                    nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort),
                    VRC_PORT.ToString());
            }
        }
        catch (InvalidOperationException) // No matching element
        {
            return;
        }
            
    }

    public void Teardown()
    {
        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery teardown called");
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _service.Dispose();
        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery teardown completed");
    }
}