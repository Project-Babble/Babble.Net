using Babble.Core;
using Babble.OSC;

namespace Babble.Maui;

public partial class AppShell : Shell
{
    private readonly BabbleOSC _sender;
    private readonly Thread _thread;

    public AppShell()
    {
        InitializeComponent();

        BabbleCore.Instance.Start();

        // TODO Pass in user's Quest headset address here!
        const string _ip = @"192.168.0.75";
        _sender = new BabbleOSC(); // _ip
        _thread = new Thread(new ThreadStart(Update));
        _thread.Start();
    }

    private void Update()
    {
        while (true)
        {
            if (!BabbleCore.Instance.GetExpressionData(out var expressions))
                goto End;

            // Assign Babble.Core data to Babble.Osc
            foreach (var exp in expressions)
                BabbleOSC.Expressions.SetByKey1(exp.Key, exp.Value);

            End:
            Thread.Sleep(10);
        }
    }
}
