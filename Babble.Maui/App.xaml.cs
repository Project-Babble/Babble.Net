using Babble.Core;
using Rug.Osc;
using System.Net;

namespace Babble.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }

   
}
