using Avalonia.Controls;
using Babble.Avalonia.ViewModels;
using Babble.Core;
using System.ComponentModel;

namespace Babble.Avalonia;

public partial class AlgoView : UserControl
{
    private readonly AlgoSettingsViewModel _viewModel;

    public AlgoView()
    {
        InitializeComponent();

        _viewModel = new AlgoSettingsViewModel();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = BabbleCore.Instance.Settings;
        switch (e.PropertyName)
        {
            case nameof(_viewModel.ModelFileEntryText):
                BabbleCore.Instance.Settings.UpdateSetting<string>(nameof(settings.GeneralSettings.GuiModelFile), _viewModel.ModelFileEntryText);
                break;
            case nameof(_viewModel.InferenceThreadsEntryText):
                BabbleCore.Instance.Settings.UpdateSetting<int>(nameof(settings.GeneralSettings.GuiInferenceThreads), _viewModel.InferenceThreadsEntryText.ToString());
                break;
            case nameof(_viewModel.UseGpu):
                BabbleCore.Instance.Settings.UpdateSetting<bool>(nameof(settings.GeneralSettings.GuiUseGpu), _viewModel.UseGpu.ToString());
                break;
            case nameof(_viewModel.GpuIndexEntryText):
                BabbleCore.Instance.Settings.UpdateSetting<int>(nameof(settings.GeneralSettings.GuiGpuIndex), _viewModel.GpuIndexEntryText.ToString());
                break;
            case nameof(_viewModel.ModelOutputMultiplierEntryText):
                BabbleCore.Instance.Settings.UpdateSetting<double>(nameof(settings.GeneralSettings.GuiMultiply), _viewModel.ModelOutputMultiplierEntryText.ToString());
                break;
            case nameof(_viewModel.CalibrationDeadzoneEntryText):
                BabbleCore.Instance.Settings.UpdateSetting<double>(nameof(settings.GeneralSettings.CalibDeadzone), _viewModel.CalibrationDeadzoneEntryText.ToString());
                break;
            case nameof(_viewModel.MinFrequencyCutoffEntryText):
                BabbleCore.Instance.Settings.UpdateSetting<double>(nameof(settings.GeneralSettings.GuiMinCutoff), _viewModel.MinFrequencyCutoffEntryText.ToString());
                break;
            case nameof(_viewModel.SpeedCoefficientEntryText):
                BabbleCore.Instance.Settings.UpdateSetting<double>(nameof(settings.GeneralSettings.GuiSpeedCoefficient), _viewModel.SpeedCoefficientEntryText.ToString());
                break;
        }

        settings.Save();
    }
}