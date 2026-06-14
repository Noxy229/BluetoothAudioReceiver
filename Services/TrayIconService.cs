using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using BluetoothAudioReceiver.Models;

namespace BluetoothAudioReceiver.Services;

/// <summary>
/// Manages the system tray icon and context menu.
/// </summary>
public class TrayIconService : IDisposable
{
    private NotifyIcon? _trayIcon;
    private ToolStripMenuItem? _showItem;
    private ToolStripMenuItem? _settingsItem;
    private ToolStripMenuItem? _exitItem;
    private readonly Window _mainWindow;
    private readonly AppSettings _settings;
    
    public event EventHandler? ShowWindowRequested;
    public event EventHandler? ExitRequested;
    public event EventHandler? SettingsRequested;
    
    public TrayIconService(Window mainWindow, AppSettings settings)
    {
        _mainWindow = mainWindow;
        _settings = settings;
        InitializeTrayIcon();
    }
    
    private void InitializeTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = CreateIcon(),
            Visible = true,
            Text = LocalizationService.Instance.Get("FullAppTitle")
        };
        
        // Create context menu
        var contextMenu = new ContextMenuStrip();
        
        _showItem = new ToolStripMenuItem(LocalizationService.Instance.Get("Show"));
        _showItem.Click += (s, e) => ShowWindowRequested?.Invoke(this, EventArgs.Empty);
        _showItem.Font = new Font(_showItem.Font, System.Drawing.FontStyle.Bold);
        contextMenu.Items.Add(_showItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        _settingsItem = new ToolStripMenuItem(LocalizationService.Instance.Get("Settings"));
        _settingsItem.Click += (s, e) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        contextMenu.Items.Add(_settingsItem);
        
        contextMenu.Items.Add(new ToolStripSeparator());
        
        _exitItem = new ToolStripMenuItem(LocalizationService.Instance.Get("Exit"));
        _exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
        contextMenu.Items.Add(_exitItem);
        
        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (s, e) => ShowWindowRequested?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Creates a simple Bluetooth-style icon programmatically.
    /// </summary>
    private Icon CreateIcon()
    {
        using var bitmap = new Bitmap(32, 32);
        using var g = Graphics.FromImage(bitmap);
        
        // Blue background circle
        using var blueBrush = new SolidBrush(Color.FromArgb(0, 120, 212));
        g.FillEllipse(blueBrush, 2, 2, 28, 28);
        
        // White Bluetooth symbol
        using var whitePen = new Pen(Color.White, 2);
        // Main vertical line
        g.DrawLine(whitePen, 16, 6, 16, 26);
        // Top arrow
        g.DrawLine(whitePen, 16, 6, 22, 12);
        g.DrawLine(whitePen, 22, 12, 10, 20);
        // Bottom arrow
        g.DrawLine(whitePen, 16, 26, 22, 20);
        g.DrawLine(whitePen, 22, 20, 10, 12);
        
        return Icon.FromHandle(bitmap.GetHicon());
    }
    
    /// <summary>
    /// Updates the tray icon tooltip text.
    /// </summary>
    public void UpdateStatus(string status)
    {
        if (_trayIcon != null)
        {
            SetTrayText($"{LocalizationService.Instance.Get("FullAppTitle")} - {status}");
        }
    }

    public void RefreshLocalization()
    {
        if (_trayIcon == null)
        {
            return;
        }

        _showItem!.Text = LocalizationService.Instance.Get("Show");
        _settingsItem!.Text = LocalizationService.Instance.Get("Settings");
        _exitItem!.Text = LocalizationService.Instance.Get("Exit");
        SetTrayText(LocalizationService.Instance.Get("FullAppTitle"));
    }
    
    /// <summary>
    /// Shows a balloon notification.
    /// </summary>
    public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        if (_trayIcon != null && _settings.ShowNotifications)
        {
            _trayIcon.ShowBalloonTip(3000, title, message, icon);
        }
    }

    private void SetTrayText(string text)
    {
        if (_trayIcon != null)
        {
            _trayIcon.Text = text.Length > 63 ? text[..60] + "..." : text;
        }
    }
    
    public void Dispose()
    {
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }
    }
}
