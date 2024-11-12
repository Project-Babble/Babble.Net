using Avalonia.Controls;
using Avalonia.Interactivity;
using Babble.Avalonia.Scripts;
using Babble.Avalonia.ViewModels;

namespace Babble.Avalonia;

public partial class AboutView : UserControl, IIsVisible
{
    public bool Visible { get => _isVisible; }
    private bool _isVisible;

    private readonly AboutViewModel _viewModel;
    
    public AboutView()
    {
        InitializeComponent();
        Loaded += CamView_OnLoaded;
        Unloaded += CamView_Unloaded;

        _viewModel = new AboutViewModel();
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
}