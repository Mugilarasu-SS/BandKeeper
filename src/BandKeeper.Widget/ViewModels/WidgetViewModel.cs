using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BandKeeper.Widget.ViewModels;

public sealed class WidgetViewModel : INotifyPropertyChanged
{
    private string _downloadText = "DL: 0 B/s";
    private string _uploadText = "UL: 0 B/s";

    public string DownloadText
    {
        get => _downloadText;
        set
        {
            if (_downloadText == value) return;
            _downloadText = value;
            OnPropertyChanged();
        }
    }

    public string UploadText
    {
        get => _uploadText;
        set
        {
            if (_uploadText == value) return;
            _uploadText = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
