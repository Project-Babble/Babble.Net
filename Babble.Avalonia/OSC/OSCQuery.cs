using Babble.Avalonia.Scripts;
using Babble.Core;
using Babble.Core.Scripts;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using VRC.OSCQuery;

namespace VRCFTReceiver;

public class OSCQuery
{
    public event Action<VRC.OSCQuery.OSCQueryNode> OnAvatarChange;

    private OSCQueryService service = null!;
    private readonly List<OSCQueryServiceProfile> profiles = [];

    private static readonly Regex VRChatClientRegex = new Regex(@"VRChat-Client-[A-Za-z0-9]{6}$", RegexOptions.Compiled);
    private CancellationTokenSource _cancellationTokenSource;
    private Thread _oscQueryThread;
    private string _lastAvatarID = string.Empty;

    public OSCQuery(IPAddress hostIP, int udpPort)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _oscQueryThread = new Thread(() => RunOSCQuery(hostIP, udpPort));
        _oscQueryThread.Start();
    }

    private void RunOSCQuery(IPAddress hostIP, int udpPort)
    {
        var tcpPort = Extensions.GetAvailableTcpPort();

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

        service.OnOscQueryServiceAdded += AddProfileToList;

        StartAutoRefreshServices(5000, _cancellationTokenSource.Token);
    }

    private void AddProfileToList(OSCQueryServiceProfile profile)
    {
        if (profiles.Select(profile => profile.name).Contains(profile.name) || profile.port == service.TcpPort)
        {
            return;
        }
        lock (profiles)
        {
            profiles.Add(profile);
        }
        BabbleCore.Instance.Logger.LogInformation($"[VRCFTReceiver] Added {profile.name} to list of OSCQuery profiles, at address http://{profile.address}:{profile.port}");
    }

    private void StartAutoRefreshServices(double interval, CancellationToken cancellationToken)
    {
        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery start StartAutoRefreshServices");
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (BabbleCore.Instance.Settings.GeneralSettings.GuiForceRelevancy)
                {
                    try
                    {
                        service.RefreshServices();
                        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery RefreshedServices");
                        await PollVRChatParameters();
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        BabbleCore.Instance.Logger.LogError($"[VRCFTReceiver] Error in AutoRefreshServices: {ex.Message}");
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(interval), cancellationToken);
            }
        }, cancellationToken);
    }

    private async Task PollVRChatParameters()
    {
        OSCQueryServiceProfile vrcProfile;
        if (profiles.Count == 0) return;

        try
        {
            // This is so wrong
            vrcProfile = profiles.First(profile => VRChatClientRegex.IsMatch(profile.name));
        }
        catch (InvalidOperationException e) // No matching element
        {
            return;
        }

        if (vrcProfile is not null)
        {
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
            
            BabbleCore.Instance.Settings.UpdateSetting<string>(
                nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress),
                vrcProfile.address.ToString());
            BabbleCore.Instance.Settings.UpdateSetting<int>(
                nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort),
                vrcProfile.port.ToString());
            BabbleCore.Instance.Settings.Save();

            var tree = await Extensions.GetOSCTree(vrcProfile.address, vrcProfile.port);
            if (tree is null) return;
            if (tree.Contents is null) return;

            var avatar = tree.Contents["avatar"];
            if (avatar is null) return;
            if (avatar.Contents is null) return;

            if (avatar.Contents["change"] is not null)
            {
                var currentAvatarID = (string)avatar.Contents["change"].Value.First();
                if (_lastAvatarID != currentAvatarID)
                {
                    _lastAvatarID = currentAvatarID;

                    // Convert VRC to VRCFT Query Node
                    // VRCFaceTracking.Core.OSC.Query.OscQueryNode vrcftQueryNode = ConvertOscQueryNodeTree(avatar);
                    OnAvatarChange?.Invoke(avatar);
                }
            }
        }
    }

    //private VRCFaceTracking.Core.OSC.Query.OscQueryNode ConvertOscQueryNodeTree(VRC.OSCQuery.OSCQueryNode rootNode)
    //{
    //    VRCFaceTracking.Core.OSC.Query.OscQueryNode vrcftQueryNode = new();
    //    vrcftQueryNode.Value = rootNode.Value;
    //    vrcftQueryNode.Access = AccessValues.ReadWrite;
    //    vrcftQueryNode.Description = rootNode.Description;
    //    vrcftQueryNode.OscType = rootNode.OscType;
    //    vrcftQueryNode.FullPath = rootNode.FullPath;

    //    vrcftQueryNode.Contents = new();

    //    if (rootNode.Contents is not null)
    //    {
    //        foreach (var child in rootNode.Contents)
    //        {
    //            vrcftQueryNode.Contents.Add(child.Key, ConvertOscQueryNodeTree(child.Value));
    //        }
    //    }

    //    return vrcftQueryNode;
    //}

    public void Teardown()
    {
        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery teardown called");
        _cancellationTokenSource.Cancel();
        _oscQueryThread.Join(); // Wait for the thread to finish
        _cancellationTokenSource.Dispose();
        service.Dispose();
        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery teardown completed");
    }
}