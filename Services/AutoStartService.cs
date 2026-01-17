using System;
using Microsoft.Win32;

namespace BluetoothAudioReceiver.Services;

/// <summary>
/// Service for managing Windows autostart registry entry.
/// </summary>
public static class AutoStartService
{
    private const string AppName = "BluetoothAudioReceiver";
    private const string RegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    
    /// <summary>
    /// Enables or disables autostart with Windows.
    /// </summary>
    /// <param name="enable">True to enable autostart, false to disable.</param>
    /// <param name="startMinimized">If true, adds --minimized argument.</param>
    public static void SetAutoStart(bool enable, bool startMinimized = false)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
            if (key == null) return;
            
            if (enable)
            {
                var exePath = Environment.ProcessPath ?? 
                    System.IO.Path.Combine(AppContext.BaseDirectory, "BluetoothAudioReceiver.exe");
                var command = startMinimized ? $"\"{exePath}\" --minimized" : $"\"{exePath}\"";
                key.SetValue(AppName, command);
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch
        {
            // Ignore registry errors (e.g., permission issues)
        }
    }
    
    /// <summary>
    /// Checks if autostart is currently enabled.
    /// </summary>
    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }
}
