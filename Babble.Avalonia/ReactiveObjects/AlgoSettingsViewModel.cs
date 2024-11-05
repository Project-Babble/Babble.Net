using CommunityToolkit.Mvvm.ComponentModel;

namespace Babble.Avalonia.ReactiveObjects;

public partial class AlgoSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string modelFile;

    [ObservableProperty]
    private int inferenceThreads;

    [ObservableProperty]
    private string runtime;

    [ObservableProperty]
    private int gpuIndex;

    [ObservableProperty]
    private bool useGpu;

    [ObservableProperty]
    private double modelOutputMultiplier;

    [ObservableProperty]
    private double calibrationDeadzone;

    [ObservableProperty]
    private double minFrequencyCutoff;

    [ObservableProperty]
    private double speedCoefficient;

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
}
