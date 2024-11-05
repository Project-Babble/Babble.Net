using Avalonia.Controls;
using Babble.Avalonia.ViewModels;

namespace Babble.Avalonia;

public partial class About : UserControl
{
    private readonly AboutViewModel _viewModel;

    public About()
    {
        InitializeComponent();

        _viewModel = new AboutViewModel();
        DataContext = _viewModel;
    }
}