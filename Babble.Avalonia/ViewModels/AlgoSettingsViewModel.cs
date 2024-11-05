using Babble.Core;
using Babble.Locale;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ViewModels;

public partial class AlgoSettingsViewModel : ObservableObject, ILocalizable
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

    public string ModelSettingsText { get; private set; }

    public string ModelFileText { get; private set; }

    public string InferenceThreadsText { get; private set; }

    public string RuntimeText { get; private set; }

    public string UseGPUText { get; private set; }

    public string GPUIndexText { get; private set; }

    public string ModelOutputMultiplierText { get; private set; }

    public string CalibrationDeadzoneText { get; private set; }

    public string OneEuroFilterText { get; private set; }

    public string MinFrequencyCutoffText { get; private set; }

    public string SpeedCoefficientText { get; private set; }

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

        Localize();
        LocaleManager.OnLocaleChanged += Localize;
    }

    public void Localize()
    {
        ModelSettingsText = LocaleManager.Instance["algorithm.header"];
        ModelFileText = LocaleManager.Instance["algorithm.modelFile"];
        InferenceThreadsText = LocaleManager.Instance["algorithm.inferenceThreads"];
        RuntimeText = LocaleManager.Instance["algorithm.runtime"];
        UseGPUText = LocaleManager.Instance["algorithm.useGPU"];
        GPUIndexText = LocaleManager.Instance["algorithm.GPUIndex"];
        ModelOutputMultiplierText = LocaleManager.Instance["algorithm.modelOutputMultiplier"];
        CalibrationDeadzoneText = LocaleManager.Instance["algorithm.calibrationDeadzone"];
        OneEuroFilterText = LocaleManager.Instance["algorithm.oneEuroFilterParameters"];
        MinFrequencyCutoffText = LocaleManager.Instance["algorithm.minFrequencyCutoff"];
        SpeedCoefficientText = LocaleManager.Instance["algorithm.speedCoefficient"];
    }
}
