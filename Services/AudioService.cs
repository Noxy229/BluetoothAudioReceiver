using System;
using System.Threading;
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
    private readonly SemaphoreSlim _connectionGate = new(1, 1);
    private bool _disposed;
    private bool _isConnected;
    private AudioPlaybackConnection? _audioConnection;
    private string? _currentDeviceId;
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 500;
    
    public event EventHandler<bool>? ConnectionStateChanged;
    public event EventHandler<string>? StreamingStateChanged;
    public event EventHandler<string>? ErrorOccurred;
    
    public bool IsConnected => _isConnected;
    public bool IsStreaming { get; private set; }
    public string? CurrentDeviceId => _currentDeviceId;
    
    /// <summary>
    /// Opens an audio playback connection to the specified Bluetooth device.
    /// Includes retry logic for more reliable connections.
    /// </summary>
    public async Task<bool> OpenConnectionAsync(string deviceId)
    {
        await _connectionGate.WaitAsync();
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ReleaseConnection();
            
            // Retry logic for more reliable connections
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                if (_disposed) return false;
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
                    ReleaseConnection();
                    if (_disposed) return false;
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
        finally
        {
            _connectionGate.Release();
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
        
        var connection = _audioConnection;
        _currentDeviceId = deviceId;
        
        // Subscribe to state changes (event-driven, no polling!)
        _audioConnection.StateChanged += OnConnectionStateChanged;
        
        // Start the audio connection
        await connection.StartAsync();
        if (_disposed) return false;
        
        // Open the connection to begin receiving audio
        var result = await connection.OpenAsync();
        if (_disposed) return false;
        
        if (result.Status != AudioPlaybackConnectionOpenResultStatus.Success)
        {
            // Extended error info
            var extendedError = result.ExtendedError?.Message ?? "No extended error";
            
            if (attempt == MaxRetries)
            {
                ErrorOccurred?.Invoke(this, $"Failed to open audio connection: {result.Status}");
            }
            
            // Clean up failed connection
            ReleaseConnection();
            return false;
        }
        
        // Wait for the audio subsystem to initialize and reach the Opened state
        // This is faster than a fixed delay if the state updates quickly
        await WaitForStateAsync(connection, AudioPlaybackConnectionState.Opened, 500);
        if (_disposed) return false;
        
        // Force a state update to ensure we're in the right state
        IsStreaming = connection.State == AudioPlaybackConnectionState.Opened;
        _isConnected = true;
        ConnectionStateChanged?.Invoke(this, true);
        StreamingStateChanged?.Invoke(this, IsStreaming ? "Streaming" : "Connected");
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

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

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
        await _connectionGate.WaitAsync();
        try
        {
            ReleaseConnection();
        }
        finally
        {
            _connectionGate.Release();
        }
    }
            
    private void ReleaseConnection()
    {
        var connection = _audioConnection;
        _audioConnection = null;
        _currentDeviceId = null;
        IsStreaming = false;
        var wasConnected = _isConnected;
        _isConnected = false;
        if (connection != null)
        {
            connection.StateChanged -= OnConnectionStateChanged;
            connection.Dispose();
        }
        if (wasConnected) ConnectionStateChanged?.Invoke(this, false);
    }
    
    private void OnConnectionStateChanged(AudioPlaybackConnection sender, object args)
    {
        if (_disposed || !ReferenceEquals(sender, _audioConnection)) return;
        if (_isConnected && sender.State == AudioPlaybackConnectionState.Closed)
        {
            ReleaseConnection();
            return;
        }
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
        if (_disposed) return;
        _disposed = true;
        ReleaseConnection();
    }
}
