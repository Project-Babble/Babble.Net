using Babble.Avalonia.ReactiveObjects;
using Babble.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Babble.Avalonia.ViewModels;

public partial class CalibrationViewModel : ViewModelBase
{
    public ObservableCollection<CalibrationItem> CalibrationItems { get; }

    public CalibrationViewModel()
    {
        CalibrationItems = [];
        var config = JsonConvert.DeserializeObject<CalibrationItem[]>(BabbleCore.Instance.Settings.GeneralSettings.CalibArray)!;
        foreach (var item in config)
        {
            var ci = new CalibrationItem
            {
                ShapeName = item.ShapeName,
                Min = item.Min,
                Max = item.Max,
            };
            ci.PropertyChanged += SaveCalibrations;
            CalibrationItems.Add(ci);
        }
    }

    private void SaveCalibrations(object? sender, PropertyChangedEventArgs e)
    {
        BabbleCore.Instance.Settings.UpdateSetting<string>(
            nameof(BabbleCore.Instance.Settings.GeneralSettings.CalibArray),
            JsonConvert.SerializeObject(CalibrationItems));
    }
}
