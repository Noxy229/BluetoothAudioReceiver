using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
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
    private bool _isExiting;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Load settings
        _settings = AppSettings.Load();
        
        LocalizationService.Instance.CurrentLanguage = _settings.Language;

        // Initialize ViewModel
        _viewModel = new MainViewModel(_settings);
        DataContext = _viewModel;
        
        // Initialize tray icon
        _trayService = new TrayIconService(this, _settings);
        _trayService.ShowWindowRequested += OnShowWindowRequested;
        _trayService.ExitRequested += OnExitRequested;
        _trayService.SettingsRequested += OnSettingsRequested;
        
        // Subscribe to connection state changes for tray notifications
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
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
            _trayService?.ShowNotification(LocalizationService.Instance["Minimized"], LocalizationService.Instance["AppRunningInTray"]);
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
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(new Action(() => OnViewModelPropertyChanged(sender, e)));
            return;
        }
        if (_viewModel == null || _trayService == null) return;
        
        if (e.PropertyName == nameof(MainViewModel.Status))
        {
            _trayService.UpdateStatus(_viewModel.Status);
        }
        else if (e.PropertyName == nameof(MainViewModel.IsConnected))
        {
            if (_viewModel.IsConnected)
            {
                _trayService.ShowNotification(LocalizationService.Instance["Connected"],
                    string.Format(LocalizationService.Instance["AudioReadyFrom"], _viewModel.SelectedDevice?.Name));
                
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
            _trayService?.ShowNotification(LocalizationService.Instance["Minimized"], LocalizationService.Instance["AppRunningInTray"]);
        }
        else
        {
            // Actually closing - clean up
            _viewModel?.Dispose();
            _trayService?.Dispose();
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
            // The dialog edits the shared settings instance used by the tray service.
        }
    }
    
    #endregion
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
