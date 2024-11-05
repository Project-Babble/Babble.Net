using Avalonia.Controls;
using Babble.Avalonia.ViewModels;
using Babble.Core;
using Babble.Locale;
using System;
using System.ComponentModel;

namespace Babble.Avalonia;

public partial class SettingsView : UserControl
{
    private readonly SettingsViewModel _viewModel;
    private readonly ComboBox comboBox;

    public SettingsView()
    {
        InitializeComponent();

        comboBox = this.Find<ComboBox>("LanguageCombo")!;
        
        comboBox!.Items.Clear();
        foreach (var item in LocaleManager.Instance.GetLanguages())
        {
            comboBox.Items.Add(item);
        }

        comboBox.SelectedIndex = 0;
        comboBox.SelectionChanged += ComboBox_SelectionChanged;

        _viewModel = new SettingsViewModel();
        DataContext = _viewModel;

        _viewModel.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = BabbleCore.Instance.Settings;
        switch (e.PropertyName)
        {
            case nameof(_viewModel.CheckForUpdates):
                BabbleCore.Instance.Settings.UpdateSetting<bool>(nameof(settings.GeneralSettings.GuiUpdateCheck), _viewModel.CheckForUpdates.ToString());
                break;
            case nameof(_viewModel.LocationPrefix):
                BabbleCore.Instance.Settings.UpdateSetting<string>(nameof(settings.GeneralSettings.GuiOscLocation), _viewModel.LocationPrefix);
                break;
            case nameof(_viewModel.IpAddress):
                BabbleCore.Instance.Settings.UpdateSetting<string>(nameof(settings.GeneralSettings.GuiOscAddress), _viewModel.IpAddress);
                break;
            case nameof(_viewModel.Port):
                BabbleCore.Instance.Settings.UpdateSetting<int>(nameof(settings.GeneralSettings.GuiOscPort), _viewModel.Port.ToString());
                break;
            case nameof(_viewModel.ReceiverPort):
                BabbleCore.Instance.Settings.UpdateSetting<int>(nameof(settings.GeneralSettings.GuiOscReceiverPort), _viewModel.ReceiverPort.ToString());
                break;
            case nameof(_viewModel.RecalibrateAddress):
                BabbleCore.Instance.Settings.UpdateSetting<string>(nameof(settings.GeneralSettings.GuiOscRecalibrateAddress), _viewModel.RecalibrateAddress);
                break;
            case nameof(_viewModel.UseRedChannel):
                BabbleCore.Instance.Settings.UpdateSetting<bool>(nameof(settings.GeneralSettings.GuiUseRedChannel), _viewModel.UseRedChannel.ToString());
                break;
            case nameof(_viewModel.XResolution):
                BabbleCore.Instance.Settings.UpdateSetting<int>(nameof(settings.GeneralSettings.GuiCamResolutionX), _viewModel.XResolution.ToString());
                break;
            case nameof(_viewModel.YResolution):
                BabbleCore.Instance.Settings.UpdateSetting<int>(nameof(settings.GeneralSettings.GuiCamResolutionY), _viewModel.YResolution.ToString());
                break;
            case nameof(_viewModel.Framerate):
                BabbleCore.Instance.Settings.UpdateSetting<double>(nameof(settings.GeneralSettings.GuiCamFramerate), _viewModel.Framerate.ToString());
                break;
        }
        settings.Save();
    }

    private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var lang = LocaleManager.Instance.GetLanguages()[comboBox.SelectedIndex];
        LocaleManager.Instance.ChangeLanguage(lang);
        BabbleCore.Instance.Settings.Save();
    }
}