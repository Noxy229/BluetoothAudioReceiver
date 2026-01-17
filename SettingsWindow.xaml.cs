using System.Windows;
using System.Windows.Input;
using BluetoothAudioReceiver.Models;
using BluetoothAudioReceiver.Services;

namespace BluetoothAudioReceiver;

/// <summary>
/// Settings window code-behind.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly AppSettings _originalSettings;
    private readonly VolumeService _volumeService;
    
    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        
        _settings = settings;
        _volumeService = new VolumeService();
        
        // Sync volume with current system volume
        _settings.Volume = _volumeService.GetVolume();
        
        // Store original values to detect changes
        _originalSettings = new AppSettings
        {
            Volume = settings.Volume,
            AutoStart = settings.AutoStart,
            StartMinimized = settings.StartMinimized,
            MinimizeToTray = settings.MinimizeToTray,
            AutoConnect = settings.AutoConnect,
            ShowNotifications = settings.ShowNotifications
        };
        
        DataContext = _settings;
        
        // Subscribe to volume slider changes for real-time preview
        VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
    }
    
    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Apply volume in real-time as user drags slider
        _volumeService.SetVolume((int)e.NewValue);
    }
    
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }
    
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Update autostart registry if changed
        if (_settings.AutoStart != _originalSettings.AutoStart || 
            _settings.StartMinimized != _originalSettings.StartMinimized)
        {
            AutoStartService.SetAutoStart(_settings.AutoStart, _settings.StartMinimized);
        }
        
        // Apply final volume
        _volumeService.SetVolume(_settings.Volume);
        
        // Save settings to file
        _settings.Save();
        
        _volumeService.Dispose();
        DialogResult = true;
        Close();
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // Restore original volume
        _volumeService.SetVolume(_originalSettings.Volume);
        
        // Restore original values
        _settings.Volume = _originalSettings.Volume;
        _settings.AutoStart = _originalSettings.AutoStart;
        _settings.StartMinimized = _originalSettings.StartMinimized;
        _settings.MinimizeToTray = _originalSettings.MinimizeToTray;
        _settings.AutoConnect = _originalSettings.AutoConnect;
        _settings.ShowNotifications = _originalSettings.ShowNotifications;
        
        _volumeService.Dispose();
        DialogResult = false;
        Close();
    }
}
