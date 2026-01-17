namespace BluetoothAudioReceiver.Models;

/// <summary>
/// Represents a paired Bluetooth device that can be used for audio playback.
/// </summary>
public class BluetoothDevice
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public bool IsAudioStreaming { get; set; }
    
    public string DisplayStatus => IsAudioStreaming ? "Streaming" : (IsConnected ? "Connected" : "Paired");
}
