using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public bool checkForUpdates;

    [ObservableProperty]
    public string locationPrefix;

    [ObservableProperty]
    public string address;

    [ObservableProperty]
    public int port;

    [ObservableProperty]
    public bool receiveFunctions;

    [ObservableProperty]
    public int receiverPort;

    [ObservableProperty]
    public string recalibrateAddress;

    [ObservableProperty]
    public bool useRedChannel;

    [ObservableProperty]
    public int xResolution;

    [ObservableProperty]
    public int yResolution;

    [ObservableProperty]
    public int framerate;

    public SettingsViewModel()
    {
        CheckForUpdates = true;
        Address = "127.0.0.1";
        Port = 8888;
        ReceiverPort = 9001;
        RecalibrateAddress = "/avatar/parameters/babblerecalibrate";
        XResolution = 0;
        YResolution = 0;
        Framerate = 0;
    }
}
