using System;
using NAudio.CoreAudioApi;

namespace BluetoothAudioReceiver.Services;

/// <summary>
/// Service for controlling system master volume using Windows Audio Session API.
/// </summary>
public class VolumeService : IDisposable
{
    private readonly MMDeviceEnumerator _deviceEnumerator;
    private MMDevice? _defaultDevice;
    
    public VolumeService()
    {
        _deviceEnumerator = new MMDeviceEnumerator();
        RefreshDevice();
    }
    
    private void RefreshDevice()
    {
        try
        {
            _defaultDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
        catch
        {
            _defaultDevice = null;
        }
    }
    
    /// <summary>
    /// Gets the current system volume (0-100).
    /// </summary>
    public int GetVolume()
    {
        try
        {
            if (_defaultDevice == null) RefreshDevice();
            if (_defaultDevice?.AudioEndpointVolume == null) return 100;
            
            return (int)(_defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }
        catch
        {
            return 100;
        }
    }
    
    /// <summary>
    /// Sets the system volume (0-100).
    /// </summary>
    public void SetVolume(int volume)
    {
        try
        {
            if (_defaultDevice == null) RefreshDevice();
            if (_defaultDevice?.AudioEndpointVolume == null) return;
            
            volume = Math.Clamp(volume, 0, 100);
            _defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = volume / 100f;
        }
        catch
        {
            // Ignore volume control errors
        }
    }
    
    /// <summary>
    /// Gets or sets mute state.
    /// </summary>
    public bool IsMuted
    {
        get
        {
            try
            {
                if (_defaultDevice == null) RefreshDevice();
                return _defaultDevice?.AudioEndpointVolume?.Mute ?? false;
            }
            catch
            {
                return false;
            }
        }
        set
        {
            try
            {
                if (_defaultDevice == null) RefreshDevice();
                if (_defaultDevice?.AudioEndpointVolume != null)
                {
                    _defaultDevice.AudioEndpointVolume.Mute = value;
                }
            }
            catch
            {
                // Ignore mute errors
            }
        }
    }
    
    public void Dispose()
    {
        _defaultDevice?.Dispose();
        _deviceEnumerator?.Dispose();
    }
}
