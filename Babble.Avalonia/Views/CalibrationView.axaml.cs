using Avalonia.Controls;
using Avalonia.Interactivity;
using Babble.Avalonia.Scripts;
using Babble.Avalonia.ViewModels;

namespace Babble.Avalonia;

public partial class CalibrationView : UserControl, IIsVisible
{
    public bool Visible { get => _isVisible; }
    private bool _isVisible;

    private readonly CalibrationViewModel _viewModel;

    public CalibrationView()
    {
        InitializeComponent();
        Loaded += CamView_OnLoaded;
        Unloaded += CamView_Unloaded;

        _viewModel = new CalibrationViewModel();
        DataContext = _viewModel;
    }

    private void CamView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _isVisible = true;
    }

    private void CamView_Unloaded(object? sender, RoutedEventArgs e)
    {
        _isVisible = false;
    }

    public void OnResetMinClicked(object? sender, RoutedEventArgs e)
    {
        foreach (var item in _viewModel.CalibrationItems)
        {
            item.Min = 0.0f;
        }
    }

    public void OnResetMaxClicked(object? sender, RoutedEventArgs e)
    {
        foreach (var item in _viewModel.CalibrationItems)
        {
            item.Max = 1.0f;
        }
    }
}