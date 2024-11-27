using Babble.Core;
using Babble.Core.Scripts;
using Microsoft.Extensions.Logging;
using System.Net;
using VRC.OSCQuery;

namespace VRCFTReceiver;

public class OSCQuery
{
    public event Action<HashSet<string>> OnAvatarChange;

    private OSCQueryService service = null!;
    private readonly List<OSCQueryServiceProfile> profiles = [];

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

        // AddParametersToEndpoint();

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

    //private void AddParametersToEndpoint()
    //{
    //    foreach (var parameter in VRCParameters)
    //    {
    //        service.AddEndpoint<float>(parameter, Attributes.AccessValues.ReadWrite, [0f]);
    //    }
    //}

    private void StartAutoRefreshServices(double interval, CancellationToken cancellationToken)
    {
        BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery start StartAutoRefreshServices");
        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    service.RefreshServices();
                    BabbleCore.Instance.Logger.LogInformation("[VRCFTReceiver] OSCQuery RefreshedServices");
                    await PollVRChatParameters();
                    await Task.Delay(TimeSpan.FromMilliseconds(interval), cancellationToken);
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
        }, cancellationToken);
    }

    private async Task PollVRChatParameters()
    {
        OSCQueryServiceProfile vrcProfile;
        if (profiles.Count == 0) return;

        try
        {
            // Replace with random OSCQuery port??
            vrcProfile = profiles.First(profile => profile.name.StartsWith("VRChat-Client"));
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

            var tree = await Extensions.GetOSCTree(vrcProfile.address, vrcProfile.port);
            if (tree is null) return;
            if (tree.Contents is null) return;

            var avatar = tree.Contents["avatar"];
            if (avatar is null) return;
            if (avatar.Contents is null) return;

            var currentAvatarID = (string)avatar.Contents["change"].Value.First();

            if (_lastAvatarID != currentAvatarID)
            {
                _lastAvatarID = currentAvatarID;
                var parameters = avatar.Contents["parameters"].Contents;
                if (parameters is null) return;

                var vrcParametersSet = new HashSet<string>();
                foreach (var item in parameters)
                {
                    vrcParametersSet.Add(item.Key);
                }

                OnAvatarChange?.Invoke(vrcParametersSet);
            }
        }
    }

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