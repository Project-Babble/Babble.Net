using Avalonia.Controls;
using Babble.Avalonia.ReactiveObjects;

namespace Babble.Avalonia;

public partial class AlgoView : UserControl
{
    private readonly AlgoSettingsViewModel _viewModel;

    public AlgoView()
    {
        InitializeComponent();

        _viewModel = new AlgoSettingsViewModel();
        DataContext = _viewModel;
    }
}