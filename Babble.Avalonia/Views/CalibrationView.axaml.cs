using Avalonia.Controls;
using Babble.Avalonia.ViewModels;
using Babble.Core;
using System.ComponentModel;

namespace Babble.Avalonia;

public partial class CalibrationView : UserControl
{
    private readonly CalibrationViewModel _viewModel;

    public CalibrationView()
    {
        InitializeComponent();

        _viewModel = new CalibrationViewModel();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        BabbleCore.Instance.Settings.Save();
    }
}