// Test doubles exercise our connection lifecycle; they do not simulate Bluetooth hardware.
namespace Windows.Foundation
{
    public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);
}
namespace Windows.Media.Audio
{
    public enum AudioPlaybackConnectionState { Closed, Opened }
    public enum AudioPlaybackConnectionOpenResultStatus { Success, UnknownFailure }
    public sealed class AudioPlaybackConnectionOpenResult
    {
        public AudioPlaybackConnectionOpenResultStatus Status { get; init; }
        public Exception? ExtendedError => null;
    }
    public sealed class AudioPlaybackConnection : IDisposable
    {
        public static Queue<AudioPlaybackConnection> Pending { get; } = new();
        public static AudioPlaybackConnection? TryCreateFromId(string _) => Pending.Count > 0 ? Pending.Dequeue() : null;
        public event Foundation.TypedEventHandler<AudioPlaybackConnection, object>? StateChanged;
        public AudioPlaybackConnectionState State { get; private set; }
        public bool ThrowOnStart { get; init; }
        public bool FailOpen { get; init; }
        public int DisposeCount { get; private set; }
        public Task StartAsync() => ThrowOnStart ? Task.FromException(new InvalidOperationException("Start failed")) : Task.CompletedTask;
        public Task<AudioPlaybackConnectionOpenResult> OpenAsync()
        {
            if (!FailOpen) { State = AudioPlaybackConnectionState.Opened; StateChanged?.Invoke(this, new object()); }
            return Task.FromResult(new AudioPlaybackConnectionOpenResult { Status = FailOpen ? AudioPlaybackConnectionOpenResultStatus.UnknownFailure : AudioPlaybackConnectionOpenResultStatus.Success });
        }
        public void RemoteClose() { State = AudioPlaybackConnectionState.Closed; StateChanged?.Invoke(this, new object()); }
        public void Dispose() => DisposeCount++;
    }
}
