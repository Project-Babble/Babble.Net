using Avalonia.Controls;
using Babble.Avalonia.ReactiveObjects;

namespace Babble.Avalonia;

public partial class SettingsView : UserControl
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView()
    {
        InitializeComponent();

        _viewModel = new SettingsViewModel();
        DataContext = _viewModel;
    }
}