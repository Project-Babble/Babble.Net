using Babble.Core;
using Babble.OSC;

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
}
