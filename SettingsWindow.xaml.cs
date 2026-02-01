using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
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
    private readonly LocalizationService _loc;
    
    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        
        _settings = settings;
        _volumeService = new VolumeService();
        _loc = LocalizationService.Instance;
        
        // Apply current language
        _loc.CurrentLanguage = settings.Language;
        
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
            ShowNotifications = settings.ShowNotifications,
            Language = settings.Language
        };
        
        DataContext = _settings;
        
        // Setup language ComboBox
        LanguageComboBox.ItemsSource = LocalizationService.AvailableLanguages;
        LanguageComboBox.SelectedValue = _settings.Language;
        
        // Subscribe to volume slider changes for real-time preview
        VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        
        // Apply localized text
        ApplyLocalization();
    }
    
    private void ApplyLocalization()
    {
        // Title bar
        Title = _loc["Settings"];
        
        // Section headers and labels (these need to be set in code since they use x:Name)
        VolumeHeader.Text = _loc["Volume"];
        AutostartHeader.Text = _loc["Autostart"];
        BehaviorHeader.Text = _loc["Behavior"];
        LanguageHeader.Text = _loc["Language"];
        
        // Accessibility names for inputs
        AutomationProperties.SetName(VolumeSlider, _loc["Volume"]);
        AutomationProperties.SetName(LanguageComboBox, _loc["Language"]);

        // Checkboxes
        AutoStartCheckBox.Content = _loc["StartWithWindows"];
        StartMinimizedCheckBox.Content = _loc["StartMinimized"];
        MinimizeToTrayCheckBox.Content = _loc["MinimizeToTray"];
        AutoConnectCheckBox.Content = _loc["AutoConnect"];
        ShowNotificationsCheckBox.Content = _loc["ShowNotifications"];
        
        // Buttons
        SaveButton.Content = _loc["Save"];
        CancelButton.Content = _loc["Cancel"];

        // Window controls
        WindowCloseButton.ToolTip = _loc["Close"];
        AutomationProperties.SetName(WindowCloseButton, _loc["Close"]);
    }
    
    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Apply volume in real-time as user drags slider
        _volumeService.SetVolume((int)e.NewValue);
    }
    
    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedValue is string langCode)
        {
            _settings.Language = langCode;
            _loc.CurrentLanguage = langCode;
            ApplyLocalization();
        }
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
        
        // Restore original language
        _loc.CurrentLanguage = _originalSettings.Language;
        
        // Restore original values
        _settings.Volume = _originalSettings.Volume;
        _settings.AutoStart = _originalSettings.AutoStart;
        _settings.StartMinimized = _originalSettings.StartMinimized;
        _settings.MinimizeToTray = _originalSettings.MinimizeToTray;
        _settings.AutoConnect = _originalSettings.AutoConnect;
        _settings.ShowNotifications = _originalSettings.ShowNotifications;
        _settings.Language = _originalSettings.Language;
        
        _volumeService.Dispose();
        DialogResult = false;
        Close();
    }
}
