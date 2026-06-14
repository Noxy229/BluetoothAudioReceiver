using System;
using System.IO;
using System.Threading;
using System.Windows;
using BluetoothAudioReceiver.Models;
using BluetoothAudioReceiver.Services;

namespace BluetoothAudioReceiver;

public partial class App : Application
{
    private static Mutex? _mutex;

    public App()
    {
        // Global exception handling
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var loc = LocalizationService.Instance;
        loc.CurrentLanguage = AppSettings.Load().Language;

        const string appName = "BluetoothAudioReceiver_SingleInstance_Mutex_Global";
        bool createdNew;

        _mutex = new Mutex(true, appName, out createdNew);

        if (!createdNew)
        {
            // App is already running
            // Optional: Bring existing window to front (requires native calls, skipping for 'Low Overhead' requirement unless asked)
            MessageBox.Show(loc["AlreadyRunning"], loc["Notice"], MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogCrash(e.Exception);
        e.Handled = true; // Prevent immediate termination if possible to show dialog

        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BluetoothAudioReceiver", "logs", "crash_log.txt");
        var loc = LocalizationService.Instance;
        MessageBox.Show($"{loc["UnexpectedError"]}\n\n{loc["CheckLogFile"]}\n{logPath}", loc["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
        Shutdown();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogCrash(ex);
        }
    }

    private void LogCrash(Exception ex)
    {
        try
        {
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BluetoothAudioReceiver", "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            string logPath = Path.Combine(logDir, "crash_log.txt");

            // Log rotation: if file > 5MB, rotate to .bak
            if (File.Exists(logPath) && new FileInfo(logPath).Length > 5 * 1024 * 1024)
            {
                string backupPath = Path.Combine(logDir, "crash_log.bak");
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                File.Move(logPath, backupPath);
            }

            string errorMessage = $"[{DateTime.Now}] CRASH REPORT:\n{ex.GetType()}: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n\nInner Exception:\n{ex.InnerException?.Message}\n--------------------------------------------------\n";
            File.AppendAllText(logPath, errorMessage);
        }
        catch
        {
            // Fallback if writing fails
        }
    }
}
