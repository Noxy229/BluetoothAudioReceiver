using System;
using System.IO;

namespace BluetoothAudioReceiver.Services;

/// <summary>
/// Centralized logging service for the application.
/// Handles secure logging to LocalAppData with rotation policies.
/// </summary>
public static class LogService
{
    private static readonly string LogDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BluetoothAudioReceiver", "logs");

    /// <summary>
    /// Gets the full path to the current log file.
    /// </summary>
    public static string LogPath => Path.Combine(LogDir, "crash_log.txt");

    /// <summary>
    /// Writes a message and optional exception details to the log file.
    /// Handles log rotation (max 5MB, 1 backup).
    /// </summary>
    public static void Log(string message, Exception? ex = null)
    {
        try
        {
            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
            }

            string logPath = LogPath;

            // Log rotation: if file > 5MB, rotate to .bak
            if (File.Exists(logPath) && new FileInfo(logPath).Length > 5 * 1024 * 1024)
            {
                string backupPath = Path.Combine(LogDir, "crash_log.bak");
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                File.Move(logPath, backupPath);
            }

            string logEntry = $"[{DateTime.Now}] {message}\n";
            if (ex != null)
            {
                logEntry += $"Exception: {ex.GetType()}: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n\nInner Exception:\n{ex.InnerException?.Message}\n";
            }
            logEntry += "--------------------------------------------------\n";

            File.AppendAllText(logPath, logEntry);
        }
        catch
        {
            // Fallback if writing fails - we can't do much here as this is the logger
        }
    }
}
