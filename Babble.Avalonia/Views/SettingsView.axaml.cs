using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Localizer.Core;
using Babble.Avalonia.Scripts;
using Babble.Avalonia.ViewModels;
using Babble.Core;
using Meadow;

namespace Babble.Avalonia;

public partial class SettingsView : UserControl, IIsVisible
{
    public bool Visible { get => _isVisible; }
    private bool _isVisible;

    private readonly SettingsViewModel _viewModel;
    private readonly ComboBox comboBox;

    public SettingsView()
    {
        InitializeComponent();
        Loaded += CamView_OnLoaded;
        Unloaded += CamView_Unloaded;

        _viewModel = new SettingsViewModel();
        DataContext = _viewModel;
        
        this.FindControl<CheckBox>("CheckForUpdates")!.IsCheckedChanged += CheckForUpdates_Changed;
        this.FindControl<CheckBox>("UseRedChannel")!.IsCheckedChanged += UseRedChannel_Changed;
        this.FindControl<CheckBox>("ForceRelevancy")!.IsCheckedChanged += ForceRelevancy_Changed;
        this.FindControl<TextBox>("LocationPrefixEntry")!.LostFocus += LocationPrefix_LostFocus;
        this.FindControl<TextBox>("IpAddressEntry")!.LostFocus += IpAddress_LostFocus;
        this.FindControl<TextBox>("PortEntry")!.LostFocus += Port_LostFocus;
        this.FindControl<TextBox>("ReceiverPortEntry")!.LostFocus += ReceiverPort_LostFocus;
        this.FindControl<TextBox>("RecalibrateAddressEntry")!.LostFocus += RecalibrateAddress_LostFocus;
        this.FindControl<TextBox>("XResolutionEntry")!.LostFocus += XResolution_LostFocus;
        this.FindControl<TextBox>("YResolutionEntry")!.LostFocus += YResolution_LostFocus;
        this.FindControl<TextBox>("FramerateEntry")!.LostFocus += Framerate_LostFocus;

        comboBox = this.Find<ComboBox>("LanguageCombo")!;
        comboBox!.Items.Clear();
        foreach (var item in LocalizerCore.Localizer.AvailableLanguages)
        {
            comboBox.Items.Add(item);
        }
        comboBox.SelectedItem = BabbleCore.Instance.Settings.GeneralSettings.GuiLanguage;
        comboBox.SelectionChanged += ComboBox_SelectionChanged;
    }

    private void CamView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _isVisible = true;
    }

    private void CamView_Unloaded(object? sender, RoutedEventArgs e)
    {
        _isVisible = false;
    }

    private void ForceRelevancy_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        BabbleCore.Instance.Settings.UpdateSetting<bool>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiForceRelevancy),
            checkBox.IsChecked.ToString()!);
        BabbleCore.Instance.Settings.Save();
    }

    private void CheckForUpdates_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        BabbleCore.Instance.Settings.UpdateSetting<bool>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiUpdateCheck),
            checkBox.IsChecked.ToString()!);
        BabbleCore.Instance.Settings.Save();
    }

    private void UseRedChannel_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox)
            return;

        BabbleCore.Instance.Settings.UpdateSetting<bool>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiUseRedChannel),
            checkBox.IsChecked.ToString()!);
        BabbleCore.Instance.Settings.Save();
    }

    private void LocationPrefix_LostFocus(object? sender, RoutedEventArgs e)
    {
        BabbleCore.Instance.Settings.UpdateSetting<string>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiOscLocation),
            _viewModel.LocationPrefix);
        BabbleCore.Instance.Settings.Save();
    }

    public void IpAddress_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsIpValid(_viewModel.IpAddress))
            return;
        
        BabbleCore.Instance.Settings.UpdateSetting<string>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiOscAddress),
            _viewModel.IpAddress);
        BabbleCore.Instance.Settings.Save();
    }

    private void Port_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsPortValid(_viewModel.Port))
            return;
        
        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiOscPort),
            _viewModel.Port.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void ReceiverPort_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsPortValid(_viewModel.ReceiverPort))
            return;

        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiOscReceiverPort),
            _viewModel.ReceiverPort.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void RecalibrateAddress_LostFocus(object? sender, RoutedEventArgs e)
    {
        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiOscRecalibrateAddress),
            _viewModel.RecalibrateAddress.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void XResolution_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsGreaterThanZero(_viewModel.XResolution))
            return;
        
        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiCamResolutionX),
            _viewModel.XResolution.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void YResolution_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsGreaterThanZero(_viewModel.YResolution))
            return;

        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiCamResolutionY),
            _viewModel.YResolution.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void Framerate_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!Validation.IsGreaterThanZero(_viewModel.Framerate))
            return;

        BabbleCore.Instance.Settings.UpdateSetting<int>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiCamFramerate),
            _viewModel.Framerate.ToString());
        BabbleCore.Instance.Settings.Save();
    }

    private void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        LocalizerCore.Localizer.SwitchLanguage(LocalizerCore.Localizer.AvailableLanguages[comboBox.SelectedIndex]);
        BabbleCore.Instance.Settings.UpdateSetting<string>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.GuiLanguage),
            LocalizerCore.Localizer.Language);
        BabbleCore.Instance.Settings.Save();
    }
}