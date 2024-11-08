using Avalonia.Controls;
using Avalonia.Interactivity;
using Babble.Avalonia.Scripts;
using Babble.Avalonia.ViewModels;
using Babble.Core;

namespace Babble.Avalonia;

public partial class AlgoView : UserControl
{
    private readonly AlgoSettingsViewModel _viewModel;

    public AlgoView()
    {
        InitializeComponent();

        _viewModel = new AlgoSettingsViewModel();
        DataContext = _viewModel;

        this.FindControl<TextBox>("ModelFileEntry")!.LostFocus += ModelFileEntry_LostFocus;
        this.FindControl<TextBox>("InferenceThreadsEntry")!.LostFocus += InferenceThreadsEntry_LostFocus;
        this.FindControl<CheckBox>("UseGpu")!.IsCheckedChanged += UseGpu_Changed;
        this.FindControl<TextBox>("GpuIndexEntry")!.LostFocus += GpuIndexEntry_LostFocus;
        this.FindControl<TextBox>("ModelOutputMultiplierEntry")!.LostFocus += ModelOutputMultiplierEntry_LostFocus;
        this.FindControl<TextBox>("CalibrationDeadzoneEntry")!.LostFocus += CalibrationDeadzoneEntry_LostFocus;
        this.FindControl<TextBox>("MinFrequencyCutoffEntry")!.LostFocus += MinFrequencyCutoffEntry_LostFocus;
        this.FindControl<TextBox>("SpeedCoefficientEntry")!.LostFocus += SpeedCoefficientEntry_LostFocus;
    }

    private void ModelFileEntry_LostFocus(object? sender, RoutedEventArgs e)
    {
        BabbleCore.Instance.Settings.UpdateSetting<string>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiModelFile),
            _viewModel.ModelFileEntryText);
        BabbleCore.Instance.Settings.Save();
    }

    private void InferenceThreadsEntry_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsGreaterThanZero(_viewModel.InferenceThreadsEntryText))
            return;

        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiInferenceThreads),
            _viewModel.InferenceThreadsEntryText.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void UseGpu_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;
        
        BabbleCore.Instance.Settings.UpdateSetting<bool>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiUseGpu),
            checkBox.IsChecked.ToString()!);
        BabbleCore.Instance.Settings.Save();
    }

    private void GpuIndexEntry_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsGreaterThanZero(_viewModel.GpuIndexEntryText))
            return;

        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiGpuIndex),
            _viewModel.GpuIndexEntryText.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void ModelOutputMultiplierEntry_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsGreaterThanZero(_viewModel.ModelOutputMultiplierEntryText))
            return;

        BabbleCore.Instance.Settings.UpdateSetting<double>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiMultiply),
            _viewModel.ModelOutputMultiplierEntryText.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void CalibrationDeadzoneEntry_LostFocus(object? sender, RoutedEventArgs e)
    {
        BabbleCore.Instance.Settings.UpdateSetting<double>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.CalibDeadzone),
            _viewModel.CalibrationDeadzoneEntryText.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void MinFrequencyCutoffEntry_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsGreaterThanZero(_viewModel.MinFrequencyCutoffEntryText))
            return;

        BabbleCore.Instance.Settings.UpdateSetting<double>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiMinCutoff),
            _viewModel.MinFrequencyCutoffEntryText.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void SpeedCoefficientEntry_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsGreaterThanZero(_viewModel.SpeedCoefficientEntryText))
            return;

        BabbleCore.Instance.Settings.UpdateSetting<double>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiSpeedCoefficient),
            _viewModel.SpeedCoefficientEntryText.ToString());
        BabbleCore.Instance.Settings.Save();
    }
}