using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class CalibrationItem : ObservableObject
{
    [ObservableProperty]
    public string shapeName;

    [ObservableProperty]
    public string leftValue;

    [ObservableProperty]
    public string rightValue;
}
