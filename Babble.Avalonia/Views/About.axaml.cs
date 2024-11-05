using Avalonia.Controls;
using Babble.Avalonia.ViewModels;
using Babble.Core;
using System.ComponentModel;

namespace Babble.Avalonia;

public partial class About : UserControl
{
    private readonly AboutViewModel _viewModel;

    public About()
    {
        InitializeComponent();

        _viewModel = new AboutViewModel();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        BabbleCore.Instance.Settings.Save();
    }
}