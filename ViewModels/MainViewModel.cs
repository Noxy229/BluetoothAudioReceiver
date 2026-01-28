using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BluetoothAudioReceiver.Models;
using BluetoothAudioReceiver.Services;

namespace BluetoothAudioReceiver.ViewModels;

/// <summary>
/// Main ViewModel for the Bluetooth Audio Receiver application.
/// Uses MVVM pattern with CommunityToolkit.Mvvm for clean separation.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly BluetoothService _bluetoothService;
    private readonly AudioService _audioService;
    private readonly object _devicesLock = new();
    private bool _isEnumerating = true;
    
    [ObservableProperty]
    private ObservableCollection<BluetoothDevice> _devices = new();
    
    [ObservableProperty]
    private BluetoothDevice? _selectedDevice;
    
    [ObservableProperty]
    private string _status = LocalizationService.Instance.Get("Idle");
    
    [ObservableProperty]
    private string _statusDetail = LocalizationService.Instance.Get("SelectDevice");
    
    [ObservableProperty]
    private bool _isConnecting;
    
    [ObservableProperty]
    private bool _isConnected;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    public MainViewModel()
    {
        _bluetoothService = new BluetoothService();
        _audioService = new AudioService();
        
        // Enable multi-threaded access to the collection
        BindingOperations.EnableCollectionSynchronization(Devices, _devicesLock);

        // Wire up events (event-driven, no polling!)
        _bluetoothService.DeviceAdded += OnDeviceAdded;
        _bluetoothService.DeviceRemoved += OnDeviceRemoved;
        _bluetoothService.EnumerationCompleted += OnEnumerationCompleted;
        // DeviceUpdated is handled automatically via INotifyPropertyChanged on the shared BluetoothDevice objects
        
        _audioService.ConnectionStateChanged += OnAudioConnectionStateChanged;
        _audioService.StreamingStateChanged += OnStreamingStateChanged;
        _audioService.ErrorOccurred += OnAudioError;
        
        // Start discovering devices
        _bluetoothService.StartWatching();
    }
    
    private void OnDeviceAdded(object? sender, BluetoothDevice device)
    {
        lock (_devicesLock)
        {
            Devices.Add(device);
            if (!_isEnumerating)
            {
                StatusDetail = string.Format(LocalizationService.Instance.Get("DevicesFound"), Devices.Count);
            }
        }
    }

    private void OnEnumerationCompleted(object? sender, EventArgs e)
    {
        lock (_devicesLock)
        {
            _isEnumerating = false;
            StatusDetail = string.Format(LocalizationService.Instance.Get("DevicesFound"), Devices.Count);
        }
    }
    
    private void OnDeviceRemoved(object? sender, string deviceId)
    {
        lock (_devicesLock)
        {
            for (int i = Devices.Count - 1; i >= 0; i--)
            {
                if (Devices[i].Id == deviceId)
                {
                    Devices.RemoveAt(i);
                    break;
                }
            }
            StatusDetail = string.Format(LocalizationService.Instance.Get("DevicesFound"), Devices.Count);
        }
    }
    
    private void OnAudioConnectionStateChanged(object? sender, bool connected)
    {
        // WPF automatically marshals PropertyChanged events for scalar properties to the UI thread
        IsConnected = connected;
        IsConnecting = false;

        if (connected)
        {
            Status = LocalizationService.Instance.Get("Connected");
            StatusDetail = string.Format(LocalizationService.Instance.Get("AudioReadyFrom"), SelectedDevice?.Name);
            ErrorMessage = null;
        }
        else
        {
            Status = LocalizationService.Instance.Get("Idle");
            StatusDetail = LocalizationService.Instance.Get("SelectDevice");
        }
    }
    
    private void OnStreamingStateChanged(object? sender, string state)
    {
        // WPF automatically marshals PropertyChanged events for scalar properties to the UI thread
        Status = LocalizationService.Instance.Get(state);
        if (state == "Streaming")
        {
            StatusDetail = string.Format(LocalizationService.Instance.Get("ReceivingAudioFrom"), SelectedDevice?.Name);
        }
    }
    
    private void OnAudioError(object? sender, string error)
    {
        // WPF automatically marshals PropertyChanged events for scalar properties to the UI thread
        ErrorMessage = error;
        IsConnecting = false;
    }
    
    [RelayCommand]
    private async Task OpenConnectionAsync()
    {
        if (SelectedDevice == null) return;
        
        IsConnecting = true;
        Status = LocalizationService.Instance.Get("Connecting");
        StatusDetail = string.Format(LocalizationService.Instance.Get("OpeningConnectionTo"), SelectedDevice.Name);
        ErrorMessage = null;
        
        await _audioService.OpenConnectionAsync(SelectedDevice.Id);
    }
    
    [RelayCommand]
    private async Task CloseConnectionAsync()
    {
        await _audioService.CloseConnectionAsync();
    }
    
    [RelayCommand]
    private void RefreshDevices()
    {
        lock (_devicesLock)
        {
            Devices.Clear();
        }
        _bluetoothService.StopWatching();

        _isEnumerating = true;
        StatusDetail = LocalizationService.Instance.Get("Scanning");
        _bluetoothService.StartWatching();
    }

    [RelayCommand]
    private void OpenBluetoothSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo("ms-settings:bluetooth") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            // Fallback: try opening generic settings if bluetooth specific fails
             try
             {
                 Process.Start(new ProcessStartInfo("ms-settings:") { UseShellExecute = true });
             }
             catch
             {
                 // Ignore if both fail
             }
        }
    }
    
    public void Dispose()
    {
        _bluetoothService.DeviceAdded -= OnDeviceAdded;
        _bluetoothService.DeviceRemoved -= OnDeviceRemoved;
        _bluetoothService.EnumerationCompleted -= OnEnumerationCompleted;
        
        _audioService.ConnectionStateChanged -= OnAudioConnectionStateChanged;
        _audioService.StreamingStateChanged -= OnStreamingStateChanged;
        _audioService.ErrorOccurred -= OnAudioError;
        
        _bluetoothService.Dispose();
        _audioService.Dispose();
    }
}
