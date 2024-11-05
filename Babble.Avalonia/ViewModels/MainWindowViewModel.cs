using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public ObservableCollection<CalibrationItem> calibrationItems;

    [ObservableProperty]
    public string selectedCalibrationMode;

    public MainWindowViewModel()
    {
        SelectedCalibrationMode = "Neutral";
        InitializeCalibrationItems();
    }

    private void InitializeCalibrationItems()
    {
        CalibrationItems = new ObservableCollection<CalibrationItem>
        {
            new CalibrationItem { ShapeName = "cheekPuffLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "cheekSuckLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "jawOpen", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "jawForward", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "jawLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "noseSneerLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "mouthFunnel", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "mouthPucker", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "mouthLeft/Right", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "mouthRollUpper", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "mouthRollLower", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "mouthShrugUpper", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "mouthShrugLower", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "mouthClose", LeftValue = "0.0", RightValue = "1.0" },
            new CalibrationItem { ShapeName = "mouthSmileLeft/Right", LeftValue = "0.0", RightValue = "1.0" }
        };
    }

    private void ExecuteResetMin()
    {
        foreach (var item in CalibrationItems)
        {
            item.LeftValue = "0.0";
        }
    }

    private void ExecuteResetMax()
    {
        foreach (var item in CalibrationItems)
        {
            item.RightValue = "1.0";
        }
    }
}
