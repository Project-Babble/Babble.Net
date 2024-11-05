using Babble.Locale;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class AlgoSettingsViewModel : ObservableObject, ILocalizable
{
    [ObservableProperty]
    public string modelFile;

    [ObservableProperty]
    public int inferenceThreads;

    [ObservableProperty]
    public string runtime;

    [ObservableProperty]
    public int gpuIndex;

    [ObservableProperty]
    public bool useGpu;

    [ObservableProperty]
    public double modelOutputMultiplier;

    [ObservableProperty]
    public double calibrationDeadzone;

    [ObservableProperty]
    public double minFrequencyCutoff;

    [ObservableProperty]
    public double speedCoefficient;

    public string ModelSettingsText => LocaleManager.Instance["algorithm.header"];

    public string ModelFileText => LocaleManager.Instance["algorithm.modelFile"];

    public string InferenceThreadsText => LocaleManager.Instance["algorithm.inferenceThreads"];

    public string RuntimeText => LocaleManager.Instance["algorithm.runtime"];

    public string UseGPUText => LocaleManager.Instance["algorithm.useGPU"];

    public string ModelOutputMultiplierText => LocaleManager.Instance["algorithm.modelOutputMultiplier"];

    public string CalibrationDeadzoneText => LocaleManager.Instance["algorithm.calibrationDeadzone"];

    public string OneEuroFilterText => LocaleManager.Instance["algorithm.oneEuroFilterParameters"];

    public string MinFrequencyCutoffText => LocaleManager.Instance["algorithm.minFrequencyCutoff"];

    public string SpeedCoefficientText => LocaleManager.Instance["algorithm.speedCoefficient"];

    public AlgoSettingsViewModel()
    {
        ModelFile = "Models/3MEFFB0E7MSE/";
        InferenceThreads = 2;
        Runtime = "ONNX";
        GpuIndex = 0;
        UseGpu = true;
        ModelOutputMultiplier = 1.0;
        CalibrationDeadzone = -0.1;
        MinFrequencyCutoff = 0.9;
        SpeedCoefficient = 0.9;
    }

    public void Localize()
    {
        
    }
}
