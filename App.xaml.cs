using System;
using System.IO;
using System.Windows;

namespace BluetoothAudioReceiver;

public partial class App : Application
{
    public App()
    {
        // Global exception handling
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        LogCrash(e.Exception);
        e.Handled = true; // Prevent immediate termination if possible to show dialog
        MessageBox.Show($"Application Crash:\n{e.Exception.Message}\n\nSee crash_log.txt in AppData for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            // Security: Write to AppData to avoid exposing sensitive info on Desktop
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string logDir = Path.Combine(appDataPath, "BluetoothAudioReceiver", "logs");
            Directory.CreateDirectory(logDir);

            string logPath = Path.Combine(logDir, "crash_log.txt");
            string errorMessage = $"[{DateTime.Now}] CRASH REPORT:\n{ex.GetType()}: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n\nInner Exception:\n{ex.InnerException?.Message}\n--------------------------------------------------\n";
            File.AppendAllText(logPath, errorMessage);
        }
        catch
        {
            // Fail silently if logging fails to prevent recursive crashes
        }
    }
}
