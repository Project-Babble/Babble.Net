using Babble.Core;
using Babble.OSC;
using CommunityToolkit.Maui.Alerts;

namespace Babble.Maui;

public partial class AppShell : Shell
{
    private static readonly HashSet<string> Whitelist = ["gui_osc_location", "gui_osc_address", "gui_osc_port", "gui_osc_receiver_port"];
    private BabbleOSC _sender;
    private Thread _thread;

    public AppShell()
    {
        InitializeComponent();

        BabbleCore.Instance.Start();
        BabbleCore.Instance.Settings.OnUpdate += OnUpdate;

        var ip = BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress;
        var remotePort = BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort;
        _sender = new BabbleOSC(ip, remotePort);
        _thread = new Thread(new ThreadStart(OSCLoop));
        _thread.Start();

        Task.Run(async () =>
        {
            if (!await CheckPermissions())
            {
                await Toast.Make("Not all permissions were accepted. Application will close.").Show();
                Application.Current.Quit();
            }
        });
    }

    private void OnUpdate(string name)
    {
        if (Whitelist.Contains(name))
        {
            _sender.Teardown();
            _thread.Join();
            var ip = BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress;
            var remotePort = BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort;
            _sender = new BabbleOSC(ip, remotePort);
            _thread = new Thread(new ThreadStart(OSCLoop));
            _thread.Start();
        }
    }

    private void OSCLoop()
    {
        while (true)
        {
            if (!BabbleCore.Instance.GetExpressionData(out var expressions))
                goto End;
            
            foreach (var exp in expressions)
                BabbleOSC.Expressions.SetByKey1(exp.Key, exp.Value);

            End:
            Thread.Sleep(10);
        }
    }

    private async Task<bool> CheckPermissions()
    {
        List<PermissionStatus> statuses = new List<PermissionStatus>();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            statuses =
           [
#if ANDROID
            await CheckPermissions<Permissions.StorageRead>(),
            await CheckPermissions<Permissions.StorageWrite>(),
            await CheckPermissions<Permissions.NetworkState>(),
            await CheckPermissions<Permissions.NearbyWifiDevices>(),
            await CheckPermissions<Permissions.Phone>(),
#endif
           ];
        });

        return statuses.All(IsGranted);
    }

    private async Task<PermissionStatus> CheckPermissions<TPermission>() where TPermission : Permissions.BasePermission, new()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<TPermission>();
        }

        return status;
    }

    private static bool IsGranted(PermissionStatus status)
    {
        return status == PermissionStatus.Granted || status == PermissionStatus.Limited;
    }
}
