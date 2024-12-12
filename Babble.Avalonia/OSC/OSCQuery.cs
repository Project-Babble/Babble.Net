using Babble.Core;
using Babble.Core.Scripts;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using VRC.OSCQuery;
using VRCFaceTracking.Core.OSC.Query;

namespace VRCFTReceiver;

public class OSCQuery
{
    public event Action<OscQueryNode> OnAvatarChange;

    private OSCQueryService service = null!;
    private HashSet<OSCQueryServiceProfile> profiles = [];

    private static readonly Regex VRChatClientRegex = new(@"VRChat-Client-[A-Za-z0-9]{6}$", RegexOptions.Compiled);
    private CancellationTokenSource _cancellationTokenSource;
    private string _lastAvatarID = string.Empty;
    private const int VRC_PORT = 9000;

    public OSCQuery(IPAddress hostIP)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        RunOSCQuery(hostIP);
    }

    private void RunOSCQuery(IPAddress hostIP)
    {
        var tcpPort = Extensions.GetAvailableTcpPort();
        var udpPort = Extensions.GetAvailableUdpPort();

        // This shit doesn't work on MacOS! It just hangs forever
        service = new OSCQueryServiceBuilder()
          .WithDiscovery(new MeaModDiscovery())
          .WithHostIP(hostIP)
          .WithTcpPort(tcpPort)
          .WithUdpPort(udpPort)
          .WithServiceName($"VRChat-Client-BabbleApp-{Utils.RandomString()}") // Yes this has to start with "VRChat-Client" https://github.com/benaclejames/VRCFaceTracking/blob/f687b143037f8f1a37a3aabf97baa06309b500a1/VRCFaceTracking.Core/mDNS/MulticastDnsService.cs#L195
          .StartHttpServer()
          .AdvertiseOSCQuery()
          .AdvertiseOSC()
          .Build();

        BabbleCore.Instance.Logger.LogInformation($"[VRCFTReceiver] Started OSCQueryService {service.ServerName} at TCP {tcpPort}, UDP {udpPort}, HTTP http://{service.HostIP}:{tcpPort}");

        service.AddEndpoint<string>("/avatar/change", Attributes.AccessValues.ReadWrite, ["default"]);

        service.OnOscQueryServiceAdded += _ => AddProfileToList();
        
        StartAutoRefreshServices(5000);
    }

    private void AddProfileToList()
    {
        profiles = service.GetOSCQueryServices();
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
                        service.RefreshServices();
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
        if (profiles == null || profiles.Count == 0) return;
        
        OSCQueryServiceProfile vrcProfile;

        try
        {
            vrcProfile = profiles.First(profile => VRChatClientRegex.IsMatch(profile.name));
            if (vrcProfile == null) return;
        }
        catch (InvalidOperationException) // No matching element
        {
            return;
        }
        
        // In VRChat, tree.Contents yields 4 elements:
        // 1) tracking
        // 2) input
        // 3) chatbox
        // 4) avatar
        // Where we care about the contents under /avatar.
        // avatar.Contents yields:
        // 1) change
        // 2) parameters
        // Where we care about parameters. Duh

        // Also, on Quest we aren't sent /avatar/change events but *are*
        // Able to read the current avatar ID. So, we'll poll this, see if it changes
        // And if it does we can fire off an event to respond to this!!
        // Oh also add like a bazillion null checks in case we're loading avis.
        
        
        var tree = await Extensions.GetOSCTree(vrcProfile.address, vrcProfile.port);
        if (tree is null) return;
        if (tree.Contents is null) return;

        if (!tree.Contents.TryGetValue("avatar", out var avatar))
            if (avatar is null)
                return;
        if (avatar.Contents is null) return;

        if (!avatar.Contents.TryGetValue("change", out var change))
            if (change is null)
                return;

        var currentAvatarID = (string)change.Value.First(); // Avatar ID
        if (_lastAvatarID != currentAvatarID)
        {
            _lastAvatarID = currentAvatarID;

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

            // Convert VRC to VRCFT Query Node
            OscQueryNode vrcftQueryNode = ConvertOscQueryNodeTree(avatar);
            OnAvatarChange?.Invoke(vrcftQueryNode);
        }
    }

    private OscQueryNode ConvertOscQueryNodeTree(OSCQueryNode rootNode)
    {
        OscQueryNode vrcftQueryNode = new()
        {
            Value = rootNode.Value,
            Access = AccessValues.ReadWrite,
            Description = rootNode.Description,
            OscType = rootNode.OscType,
            FullPath = rootNode.FullPath,
            Contents = new()
        };

        if (rootNode.Contents is not null)
        {
            foreach (var child in rootNode.Contents)
            {
                vrcftQueryNode.Contents.Add(child.Key, ConvertOscQueryNodeTree(child.Value));
            }
        }

        return vrcftQueryNode;
    }

    public void Teardown()
    {
        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery teardown called");
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        service.Dispose();
        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery teardown completed");
    }
}