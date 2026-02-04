using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using BluetoothAudioReceiver.Models;

namespace BluetoothAudioReceiver.Services;

/// <summary>
/// Service for discovering and managing Bluetooth devices that support audio playback.
/// Uses AudioPlaybackConnection.GetDeviceSelector() to get compatible devices.
/// Event-based approach to minimize resource usage.
/// </summary>
public class BluetoothService : IDisposable
{
    private DeviceWatcher? _deviceWatcher;
    private readonly Dictionary<string, BluetoothDevice> _devices = new();
    private readonly object _lock = new();
    
    public event EventHandler<BluetoothDevice>? DeviceAdded;
    public event EventHandler<string>? DeviceRemoved;
    public event EventHandler<BluetoothDevice>? DeviceUpdated;
    public event EventHandler? EnumerationCompleted;
    
    /// <summary>
    /// Gets all currently known paired Bluetooth devices.
    /// </summary>
    public IEnumerable<BluetoothDevice> GetDevices()
    {
        lock (_lock)
        {
            return new List<BluetoothDevice>(_devices.Values);
        }
    }
    
    /// <summary>
    /// Starts watching for Bluetooth devices that support audio playback.
    /// Uses AudioPlaybackConnection.GetDeviceSelector() for compatible device IDs.
    /// </summary>
    public void StartWatching()
    {
        if (_deviceWatcher != null) return;
        
        // Use AudioPlaybackConnection selector to get devices with compatible IDs
        string selector = AudioPlaybackConnection.GetDeviceSelector();
        
        _deviceWatcher = DeviceInformation.CreateWatcher(
            selector,
            new[] { "System.Devices.Aep.IsConnected" },
            DeviceInformationKind.AssociationEndpoint);
        
        _deviceWatcher.Added += OnDeviceAdded;
        _deviceWatcher.Updated += OnDeviceUpdated;
        _deviceWatcher.Removed += OnDeviceRemoved;
        _deviceWatcher.EnumerationCompleted += OnEnumerationCompleted;
        
        try
        {
            _deviceWatcher.Start();
        }
        catch (Exception)
        {
            // If Bluetooth is not available or disabled, Start() might fail.
            // Stop watching to clean up event handlers.
            StopWatching();
        }
    }
    
    /// <summary>
    /// Stops watching for Bluetooth devices.
    /// </summary>
    public void StopWatching()
    {
        if (_deviceWatcher == null) return;
        
        _deviceWatcher.Added -= OnDeviceAdded;
        _deviceWatcher.Updated -= OnDeviceUpdated;
        _deviceWatcher.Removed -= OnDeviceRemoved;
        _deviceWatcher.EnumerationCompleted -= OnEnumerationCompleted;
        
        if (_deviceWatcher.Status == DeviceWatcherStatus.Started || 
            _deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted)
        {
            _deviceWatcher.Stop();
        }
        
        _deviceWatcher = null;
    }
    
    private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation device)
    {
        var btDevice = new BluetoothDevice
        {
            Id = device.Id,
            Name = SanitizeDeviceName(device.Name),
            IsConnected = device.Properties.TryGetValue("System.Devices.Aep.IsConnected", out var connected) 
                          && connected is bool isConnected && isConnected
        };
        
        lock (_lock)
        {
            _devices[device.Id] = btDevice;
        }
        DeviceAdded?.Invoke(this, btDevice);
    }
    
    private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        BluetoothDevice? device;
        lock (_lock)
        {
            if (!_devices.TryGetValue(update.Id, out device)) return;
        }
        
        if (update.Properties.TryGetValue("System.Devices.Aep.IsConnected", out var connected))
        {
            device.IsConnected = connected is bool isConnected && isConnected;
        }
        
        DeviceUpdated?.Invoke(this, device);
    }
    
    private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        bool removed = false;
        lock (_lock)
        {
            removed = _devices.Remove(update.Id);
        }

        if (removed)
        {
            DeviceRemoved?.Invoke(this, update.Id);
        }
    }
    
    private void OnEnumerationCompleted(DeviceWatcher sender, object args)
    {
        // Enumeration complete - watcher will continue to monitor for changes
        EnumerationCompleted?.Invoke(this, EventArgs.Empty);
    }

    private string SanitizeDeviceName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return LocalizationService.Instance["UnknownDevice"];
        }

        // Remove control characters
        string sanitized = new string(name.Where(c => !char.IsControl(c)).ToArray());

        // Trim
        sanitized = sanitized.Trim();

        // Check length
        if (sanitized.Length > 100)
        {
            sanitized = sanitized.Substring(0, 100);
        }

        // Check if empty after sanitization
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return LocalizationService.Instance["UnknownDevice"];
        }

        return sanitized;
    }
    
    public void Dispose()
    {
        StopWatching();
        lock (_lock)
        {
            _devices.Clear();
        }
    }
}
