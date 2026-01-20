using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
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
    
    [ObservableProperty]
    private ObservableCollection<BluetoothDevice> _devices = new();
    
    [ObservableProperty]
    private BluetoothDevice? _selectedDevice;
    
    [ObservableProperty]
    private string _status = "Idle";
    
    [ObservableProperty]
    private string _statusDetail = "Select a device to connect";
    
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
        
        // Wire up events (event-driven, no polling!)
        _bluetoothService.DeviceAdded += OnDeviceAdded;
        _bluetoothService.DeviceRemoved += OnDeviceRemoved;
        _bluetoothService.DeviceUpdated += OnDeviceUpdated;
        
        _audioService.ConnectionStateChanged += OnAudioConnectionStateChanged;
        _audioService.StreamingStateChanged += OnStreamingStateChanged;
        _audioService.ErrorOccurred += OnAudioError;
        
        // Start discovering devices
        _bluetoothService.StartWatching();
    }
    
    private void OnDeviceAdded(object? sender, BluetoothDevice device)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            Devices.Add(device);
            StatusDetail = $"{Devices.Count} device(s) found";
        });
    }
    
    private void OnDeviceRemoved(object? sender, string deviceId)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            for (int i = Devices.Count - 1; i >= 0; i--)
            {
                if (Devices[i].Id == deviceId)
                {
                    Devices.RemoveAt(i);
                    break;
                }
            }
            StatusDetail = $"{Devices.Count} device(s) found";
        });
    }
    
    private void OnDeviceUpdated(object? sender, BluetoothDevice device)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            for (int i = 0; i < Devices.Count; i++)
            {
                if (Devices[i].Id == device.Id)
                {
                    Devices[i] = device;
                    break;
                }
            }
        });
    }
    
    private void OnAudioConnectionStateChanged(object? sender, bool connected)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            IsConnected = connected;
            IsConnecting = false;
            
            if (connected)
            {
                Status = "Connected";
                StatusDetail = $"Audio ready from {SelectedDevice?.Name}";
                ErrorMessage = null;
            }
            else
            {
                Status = "Idle";
                StatusDetail = "Select a device to connect";
            }
        });
    }
    
    private void OnStreamingStateChanged(object? sender, string state)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            Status = state;
            if (state == "Streaming")
            {
                StatusDetail = $"Receiving audio from {SelectedDevice?.Name}";
            }
        });
    }
    
    private void OnAudioError(object? sender, string error)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            ErrorMessage = error;
            IsConnecting = false;
        });
    }
    
    [RelayCommand]
    private async Task OpenConnectionAsync()
    {
        if (SelectedDevice == null) return;
        
        IsConnecting = true;
        Status = "Connecting...";
        StatusDetail = $"Opening connection to {SelectedDevice.Name}";
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
        Devices.Clear();
        _bluetoothService.StopWatching();
        _bluetoothService.StartWatching();
        StatusDetail = "Scanning for devices...";
    }
    
    public void Dispose()
    {
        _bluetoothService.DeviceAdded -= OnDeviceAdded;
        _bluetoothService.DeviceRemoved -= OnDeviceRemoved;
        _bluetoothService.DeviceUpdated -= OnDeviceUpdated;
        
        _audioService.ConnectionStateChanged -= OnAudioConnectionStateChanged;
        _audioService.StreamingStateChanged -= OnStreamingStateChanged;
        _audioService.ErrorOccurred -= OnAudioError;
        
        _bluetoothService.Dispose();
        _audioService.Dispose();
    }
}
