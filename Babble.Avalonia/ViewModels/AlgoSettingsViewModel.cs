using Babble.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ViewModels;

public partial class AlgoSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    public string modelFileEntryText;

    [ObservableProperty]
    public int inferenceThreadsEntryText;

    [ObservableProperty]
    public string runtimeEntryText;

    [ObservableProperty]
    public int gpuIndexEntryText;

    [ObservableProperty]
    public bool useGpu;

    [ObservableProperty]
    public double modelOutputMultiplierEntryText;

    [ObservableProperty]
    public double calibrationDeadzoneEntryText;

    [ObservableProperty]
    public double minFrequencyCutoffEntryText;

    [ObservableProperty]
    public double speedCoefficientEntryText;

    public AlgoSettingsViewModel()
    {
        var settings = BabbleCore.Instance.Settings;
        
        modelFileEntryText = settings.GeneralSettings.GuiModelFile;
        inferenceThreadsEntryText = settings.GeneralSettings.GuiInferenceThreads;
        useGpu = settings.GeneralSettings.GuiUseGpu;
        gpuIndexEntryText = settings.GeneralSettings.GuiGpuIndex;
        modelOutputMultiplierEntryText = settings.GeneralSettings.GuiMultiply;
        calibrationDeadzoneEntryText = settings.GeneralSettings.CalibDeadzone;
        minFrequencyCutoffEntryText = double.Parse(settings.GeneralSettings.GuiMinCutoff);
        speedCoefficientEntryText = double.Parse(settings.GeneralSettings.GuiSpeedCoefficient);
    }
}
