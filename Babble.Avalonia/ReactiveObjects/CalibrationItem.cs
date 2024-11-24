using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class CalibrationItem : ObservableObject
{
    [ObservableProperty]
    public string? shapeName;

    [ObservableProperty]
    public float min;

    [ObservableProperty]
    public float max;
}
