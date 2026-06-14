using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Data;
using System.Windows.Input;
using BluetoothAudioReceiver.Models;
using BluetoothAudioReceiver.Services;
using BluetoothAudioReceiver.ViewModels;

namespace BluetoothAudioReceiver;

/// <summary>
/// Main window code-behind with custom title bar and system tray integration.
/// </summary>
public partial class MainWindow : Window
{
    private TrayIconService? _trayService;
    private AppSettings _settings;
    private MainViewModel? _viewModel;
    private readonly LocalizationService _loc = LocalizationService.Instance;
    private bool _isExiting;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Load settings
        _settings = AppSettings.Load();
        _loc.CurrentLanguage = _settings.Language;
        
        // Initialize ViewModel
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        
        // Initialize tray icon
        _trayService = new TrayIconService(this, _settings);
        _trayService.ShowWindowRequested += OnShowWindowRequested;
        _trayService.ExitRequested += OnExitRequested;
        _trayService.SettingsRequested += OnSettingsRequested;
        
        // Subscribe to connection state changes for tray notifications
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _loc.PropertyChanged += OnLocalizationPropertyChanged;

        RefreshLocalizedUi();
        
        // Handle minimize to tray
        StateChanged += OnStateChanged;
        Closing += OnClosing;
        
        // Check for --minimized argument
        var args = Environment.GetCommandLineArgs();
        if (args.Contains("--minimized") || _settings.StartMinimized)
        {
            WindowState = WindowState.Minimized;
            if (_settings.MinimizeToTray)
            {
                Hide();
            }
        }
    }
    
    #region Custom Title Bar
    
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            DragMove();
        }
    }
    
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_settings.MinimizeToTray)
        {
            WindowState = WindowState.Minimized;
            Hide();
            _trayService?.ShowNotification(_loc["Minimized"], _loc["AppRunningInTray"]);
        }
        else
        {
            _isExiting = true;
            Close();
        }
    }
    
    #endregion
    
    #region ViewModel Events
    
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_viewModel == null || _trayService == null) return;
        
        if (e.PropertyName == nameof(MainViewModel.Status))
        {
            _trayService.UpdateStatus(_viewModel.Status);
        }
        else if (e.PropertyName == nameof(MainViewModel.IsConnected))
        {
            if (_viewModel.IsConnected)
            {
                _trayService.ShowNotification(
                    _loc["Connected"],
                    string.Format(_loc["AudioConnectionEstablished"], _viewModel.SelectedDevice?.Name));
                
                // Save last device for auto-connect
                if (_viewModel.SelectedDevice != null)
                {
                    _settings.LastDeviceId = _viewModel.SelectedDevice.Id;
                    _settings.LastDeviceName = _viewModel.SelectedDevice.Name;
                    _settings.Save();
                }
            }
        }
    }
    
    #endregion
    
    #region Window State
    
    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized && _settings.MinimizeToTray)
        {
            Hide();
        }
    }
    
    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!_isExiting && _settings.MinimizeToTray)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            Hide();
            _trayService?.ShowNotification(_loc["Minimized"], _loc["AppRunningInTray"]);
        }
        else
        {
            // Actually closing - clean up
            DisposeServices();
        }
    }
    
    #endregion
    
    #region Tray Events
    
    private void OnShowWindowRequested(object? sender, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }
    
    private void OnExitRequested(object? sender, EventArgs e)
    {
        _isExiting = true;
        DisposeServices();
        Application.Current.Shutdown();
    }
    
    private void OnSettingsRequested(object? sender, EventArgs e)
    {
        ShowSettingsWindow();
    }
    
    #endregion
    
    #region Button Handlers
    
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow();
    }
    
    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        var helpWindow = new HelpWindow { Owner = this };
        helpWindow.ShowDialog();
    }
    
    private void ShowSettingsWindow()
    {
        // Show window if hidden
        if (!IsVisible)
        {
            Show();
            WindowState = WindowState.Normal;
        }
        
        var settingsWindow = new SettingsWindow(_settings) { Owner = this };
        if (settingsWindow.ShowDialog() == true)
        {
            _settings = AppSettings.Load();
            _loc.CurrentLanguage = _settings.Language;
            RefreshLocalizedUi();
        }
    }
    
    #endregion

    private void ApplyLocalization()
    {
        Title = _loc["FullAppTitle"];
        AppTitleTextBlock.Text = _loc["AppTitle"];
        DevicesHeaderTextBlock.Text = _loc["Devices"];
        NoDevicesTextBlock.Text = _loc["NoDevicesFound"];
        ConnectButton.Content = _loc["Connect"];
        DisconnectButton.Content = _loc["Disconnect"];

        MinimizeButton.ToolTip = _loc["Minimize"];
        CloseButton.ToolTip = _loc["Close"];
        HelpButton.ToolTip = _loc["Help"];
        SettingsButton.ToolTip = _loc["Settings"];
        RefreshButton.ToolTip = _loc["Refresh"];

        AutomationProperties.SetName(MinimizeButton, _loc["Minimize"]);
        AutomationProperties.SetName(CloseButton, _loc["Close"]);
        AutomationProperties.SetName(HelpButton, _loc["Help"]);
        AutomationProperties.SetName(SettingsButton, _loc["Settings"]);
        AutomationProperties.SetName(RefreshButton, _loc["RefreshDevices"]);
    }

    private void OnLocalizationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LocalizationService.CurrentLanguage) or "Item[]")
        {
            RefreshLocalizedUi();
        }
    }

    private void RefreshLocalizedUi()
    {
        ApplyLocalization();
        _trayService?.RefreshLocalization();
        _viewModel?.RefreshLocalization();
        if (_viewModel != null)
        {
            _trayService?.UpdateStatus(_viewModel.Status);
        }
    }

    private void DisposeServices()
    {
        _loc.PropertyChanged -= OnLocalizationPropertyChanged;
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.Dispose();
            _viewModel = null;
        }

        _trayService?.Dispose();
        _trayService = null;
    }
}

/// <summary>
/// Converter to determine if the Open Connection button should be enabled.
/// </summary>
public class CanConnectConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3) return false;
        
        var hasSelectedDevice = values[0] != null;
        var isConnected = values[1] is bool connected && connected;
        var isConnecting = values[2] is bool connecting && connecting;
        
        return hasSelectedDevice && !isConnected && !isConnecting;
    }
    
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
