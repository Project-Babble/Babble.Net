using Avalonia.Controls;
using Avalonia.Interactivity;
using Babble.Avalonia.Scripts;
using Babble.Avalonia.ViewModels;

namespace Babble.Avalonia;

public partial class CalibrationView : UserControl, IIsVisible
{
    public bool Visible { get; set; }

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
        Visible = true;
    }

    private void CamView_Unloaded(object? sender, RoutedEventArgs e)
    {
        Visible = false;
    }
}