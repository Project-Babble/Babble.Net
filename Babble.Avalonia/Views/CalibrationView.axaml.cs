using Avalonia.Controls;
using Babble.Avalonia.ViewModels;

namespace Babble.Avalonia;

public partial class CalibrationView : UserControl
{
    private readonly CalibrationViewModel _viewModel;

    public CalibrationView()
    {
        InitializeComponent();

        _viewModel = new CalibrationViewModel();
        DataContext = _viewModel;
    }
}