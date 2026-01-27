using System;
using System.IO;
using System.Threading;
using System.Windows;
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
        const string appName = "BluetoothAudioReceiver_SingleInstance_Mutex_Global";
        bool createdNew;

        _mutex = new Mutex(true, appName, out createdNew);

        if (!createdNew)
        {
            // App is already running
            // Optional: Bring existing window to front (requires native calls, skipping for 'Low Overhead' requirement unless asked)
            MessageBox.Show("Bluetooth Audio Receiver läuft bereits.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogService.Log("CRASH REPORT (Dispatcher)", e.Exception);
        e.Handled = true; // Prevent immediate termination if possible to show dialog

        MessageBox.Show($"An unexpected error occurred. The application will close.\n\nPlease check the log file for details:\n{LogService.LogPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Shutdown();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogService.Log("CRASH REPORT (Domain)", ex);
        }
    }
}
