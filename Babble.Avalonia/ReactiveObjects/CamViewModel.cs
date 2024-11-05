using Avalonia.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Babble.Avalonia.ReactiveObjects;

public class CamViewModel : INotifyPropertyChanged
{
    private WriteableBitmap? _mouthBitmap;

    public WriteableBitmap? MouthBitmap
    {
        get => _mouthBitmap;
        set
        {
            if (_mouthBitmap != value)
            {
                _mouthBitmap = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
