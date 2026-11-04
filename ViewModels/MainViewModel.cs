using System;
using System.Collections.ObjectModel;
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
    private bool _disposed;
    private bool _autoConnectAttempted;
    private readonly AppSettings _settings;
    private readonly System.Windows.Threading.Dispatcher _dispatcher = Application.Current.Dispatcher;
    public bool CanChangeDevice => !IsConnecting && !IsConnected;
    
    [ObservableProperty]
    private ObservableCollection<BluetoothDevice> _devices = new();
    
    [ObservableProperty]
    private BluetoothDevice? _selectedDevice;
    
    [ObservableProperty]
    private string _status = LocalizationService.Instance.Get("Idle");
    
    [ObservableProperty]
    private string _statusDetail = LocalizationService.Instance.Get("SelectDevice");
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChangeDevice))]
    [NotifyCanExecuteChangedFor(nameof(RefreshDevicesCommand))]
    private bool _isConnecting;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanChangeDevice))]
    [NotifyCanExecuteChangedFor(nameof(RefreshDevicesCommand))]
    private bool _isConnected;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    public MainViewModel(AppSettings settings)
    {
        _settings = settings;
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
        if (_disposed) return;
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(new Action(() => OnDeviceAdded(sender, device)));
            return;
        }
        lock (_devicesLock)
        {
            for (var i = Devices.Count - 1; i >= 0; i--)
                if (Devices[i].Id == device.Id) Devices.RemoveAt(i);
            Devices.Add(device);
            if (!_isEnumerating && CanChangeDevice)
            {
                StatusDetail = string.Format(LocalizationService.Instance.Get("DevicesFound"), Devices.Count);
            }
        }
    }

    private void OnEnumerationCompleted(object? sender, EventArgs e)
    {
        if (_disposed) return;
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(new Action(() => OnEnumerationCompleted(sender, e)));
            return;
        }
        lock (_devicesLock)
        {
            _isEnumerating = false;
            if (CanChangeDevice)
                StatusDetail = string.Format(LocalizationService.Instance.Get("DevicesFound"), Devices.Count);
        }
        if (!_autoConnectAttempted && _settings.AutoConnect && !IsConnected && !IsConnecting)
        {
            _autoConnectAttempted = true;
            foreach (var device in Devices)
            {
                if (device.Id != _settings.LastDeviceId) continue;
                SelectedDevice = device;
                OpenConnectionCommand.Execute(null);
                break;
            }
        }
    }
    
    private void OnDeviceRemoved(object? sender, string deviceId)
    {
        if (_disposed) return;
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(new Action(() => OnDeviceRemoved(sender, deviceId)));
            return;
        }
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
            if (CanChangeDevice)
                StatusDetail = string.Format(LocalizationService.Instance.Get("DevicesFound"), Devices.Count);
        }
    }
    
    private void OnAudioConnectionStateChanged(object? sender, bool connected)
    {
        if (_disposed) return;
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(new Action(() => OnAudioConnectionStateChanged(sender, connected)));
            return;
        }
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
        if (_disposed) return;
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(new Action(() => OnStreamingStateChanged(sender, state)));
            return;
        }
        // WPF automatically marshals PropertyChanged events for scalar properties to the UI thread
        Status = LocalizationService.Instance.Get(state);
        if (state == "Streaming")
        {
            StatusDetail = string.Format(LocalizationService.Instance.Get("ReceivingAudioFrom"), SelectedDevice?.Name);
        }
    }
    
    private void OnAudioError(object? sender, string error)
    {
        if (_disposed) return;
        if (!_dispatcher.CheckAccess())
        {
            _dispatcher.BeginInvoke(new Action(() => OnAudioError(sender, error)));
            return;
        }
        // WPF automatically marshals PropertyChanged events for scalar properties to the UI thread
        ErrorMessage = error;
        IsConnecting = false;
    }
    
    [RelayCommand]
    private async Task OpenConnectionAsync()
    {
        if (_disposed || SelectedDevice == null || !CanChangeDevice) return;
        
        IsConnecting = true;
        Status = LocalizationService.Instance.Get("Connecting");
        StatusDetail = string.Format(LocalizationService.Instance.Get("OpeningConnectionTo"), SelectedDevice.Name);
        ErrorMessage = null;
        
        try
        {
            if (!await _audioService.OpenConnectionAsync(SelectedDevice.Id))
            {
                Status = LocalizationService.Instance.Get("Idle");
                StatusDetail = LocalizationService.Instance.Get("SelectDevice");
            }
        }
        finally
        {
            IsConnecting = false;
        }
    }
    
    [RelayCommand]
    private async Task CloseConnectionAsync()
    {
        await _audioService.CloseConnectionAsync();
    }
    
    [RelayCommand(CanExecute = nameof(CanChangeDevice))]
    private void RefreshDevices()
    {
        _bluetoothService.StopWatching();
        lock (_devicesLock)
        {
            Devices.Clear();
            SelectedDevice = null;
        }

        _isEnumerating = true;
        StatusDetail = LocalizationService.Instance.Get("Scanning");
        _bluetoothService.StartWatching();
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
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
