using CommunityToolkit.Mvvm.ComponentModel;

namespace BluetoothAudioReceiver.Models;

/// <summary>
/// Represents a paired Bluetooth device that can be used for audio playback.
/// </summary>
public partial class BluetoothDevice : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayStatus))]
    private bool _isConnected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayStatus))]
    private bool _isAudioStreaming;

    public string DisplayStatus => IsAudioStreaming ? "Streaming" : (IsConnected ? "Connected" : "Paired");
}
