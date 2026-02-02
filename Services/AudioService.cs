using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Audio;

namespace BluetoothAudioReceiver.Services;

/// <summary>
/// Service for managing Bluetooth audio playback connections (A2DP Sink).
/// Uses AudioPlaybackConnection API for efficient, event-driven audio streaming.
/// </summary>
public class AudioService : IDisposable
{
    private AudioPlaybackConnection? _audioConnection;
    private string? _currentDeviceId;
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 500;
    
    public event EventHandler<bool>? ConnectionStateChanged;
    public event EventHandler<string>? StreamingStateChanged;
    public event EventHandler<string>? ErrorOccurred;
    
    public bool IsConnected => _audioConnection != null;
    public bool IsStreaming { get; private set; }
    public string? CurrentDeviceId => _currentDeviceId;
    
    /// <summary>
    /// Opens an audio playback connection to the specified Bluetooth device.
    /// Includes retry logic for more reliable connections.
    /// </summary>
    public async Task<bool> OpenConnectionAsync(string deviceId)
    {
        try
        {
            // Close any existing connection first
            await CloseConnectionAsync();
            
            // Retry logic for more reliable connections
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var result = await TryOpenConnectionAsync(deviceId, attempt);
                    if (result)
                    {
                        return true;
                    }
                    
                    if (attempt < MaxRetries)
                    {
                        await Task.Delay(RetryDelayMs);
                    }
                }
                catch (Exception ex)
                {
                    if (attempt == MaxRetries)
                    {
                        ErrorOccurred?.Invoke(this, $"Error opening connection: {ex.Message}");
                        return false;
                    }
                    await Task.Delay(RetryDelayMs);
                }
            }
            
            ErrorOccurred?.Invoke(this, "Could not establish connection after multiple attempts.");
            return false;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error opening connection: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TryOpenConnectionAsync(string deviceId, int attempt)
    {
        // Create audio playback connection
        _audioConnection = AudioPlaybackConnection.TryCreateFromId(deviceId);
        
        if (_audioConnection == null)
        {
            if (attempt == MaxRetries)
            {
                ErrorOccurred?.Invoke(this, "Could not create audio connection. Device may not support A2DP.");
            }
            return false;
        }
        
        _currentDeviceId = deviceId;
        
        // Subscribe to state changes (event-driven, no polling!)
        _audioConnection.StateChanged += OnConnectionStateChanged;
        
        // Start the audio connection
        await _audioConnection.StartAsync();
        
        // Open the connection to begin receiving audio
        var result = await _audioConnection.OpenAsync();
        
        if (result.Status != AudioPlaybackConnectionOpenResultStatus.Success)
        {
            // Extended error info
            var extendedError = result.ExtendedError?.Message ?? "No extended error";
            
            if (attempt == MaxRetries)
            {
                ErrorOccurred?.Invoke(this, $"Failed to open audio connection: {result.Status}");
            }
            
            // Clean up failed connection
            _audioConnection.StateChanged -= OnConnectionStateChanged;
            _audioConnection.Dispose();
            _audioConnection = null;
            return false;
        }
        
        // Wait for the audio subsystem to initialize and reach the Opened state
        // This is faster than a fixed delay if the state updates quickly
        await WaitForStateAsync(_audioConnection, AudioPlaybackConnectionState.Opened, 500);
        
        // Force a state update to ensure we're in the right state
        IsStreaming = _audioConnection.State == AudioPlaybackConnectionState.Opened;
        StreamingStateChanged?.Invoke(this, IsStreaming ? "Streaming" : "Connected");
        
        ConnectionStateChanged?.Invoke(this, true);
        return true;
    }

    /// <summary>
    /// Waits for the audio connection to reach a specific state with a timeout.
    /// </summary>
    private async Task WaitForStateAsync(AudioPlaybackConnection connection, AudioPlaybackConnectionState targetState, int timeoutMs)
    {
        if (connection.State == targetState)
        {
            return;
        }

        var tcs = new TaskCompletionSource<bool>();

        TypedEventHandler<AudioPlaybackConnection, object> handler = (sender, args) =>
        {
            if (sender.State == targetState)
            {
                tcs.TrySetResult(true);
            }
        };

        connection.StateChanged += handler;

        try
        {
            // Check again in case it changed before/while we subscribed
            if (connection.State == targetState)
            {
                return;
            }

            // Optimization: Use WaitAsync to avoid uncancelled Task.Delay timers
            await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
        }
        catch (TimeoutException)
        {
            // Ignore timeout, caller will verify state
        }
        finally
        {
            connection.StateChanged -= handler;
        }
    }
    
    /// <summary>
    /// Closes the current audio playback connection.
    /// </summary>
    public async Task CloseConnectionAsync()
    {
        if (_audioConnection != null)
        {
            _audioConnection.StateChanged -= OnConnectionStateChanged;
            _audioConnection.Dispose();
            _audioConnection = null;
            _currentDeviceId = null;
            IsStreaming = false;
            
            ConnectionStateChanged?.Invoke(this, false);
        }
        
        await Task.CompletedTask;
    }
    
    private void OnConnectionStateChanged(AudioPlaybackConnection sender, object args)
    {
        // Update streaming state based on connection state
        var wasStreaming = IsStreaming;
        IsStreaming = sender.State == AudioPlaybackConnectionState.Opened;
        
        if (wasStreaming != IsStreaming)
        {
            StreamingStateChanged?.Invoke(this, IsStreaming ? "Streaming" : "Connected");
        }
    }
    
    public void Dispose()
    {
        // Clean up synchronously to ensure resources are released before app exit
        if (_audioConnection != null)
        {
            try
            {
                _audioConnection.StateChanged -= OnConnectionStateChanged;
                _audioConnection.Dispose();
            }
            catch
            {
                // Ignore errors during disposal
            }
            finally
            {
                _audioConnection = null;
            }
        }
        _currentDeviceId = null;
        IsStreaming = false;
    }
}
