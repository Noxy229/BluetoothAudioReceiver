# Sentinel Journal

## 2026-01-21 - Thread Safety in BluetoothService
**Vulnerability:** The `BluetoothService` used a `Dictionary<string, BluetoothDevice>` to store devices, which was accessed by background threads (via `DeviceWatcher` events) and the UI thread (via `GetDevices`) without synchronization. This could lead to `InvalidOperationException` or data corruption (Race Condition).
**Learning:** `DeviceWatcher` events are raised on background threads, while UI services often consume data on the main thread. Standard collections like `Dictionary` are not thread-safe.
**Prevention:** Use `lock` statements to synchronize access to shared mutable state, or use concurrent collections (e.g., `ConcurrentDictionary`) when appropriate. When returning collections, return a snapshot (copy) to avoid locking during iteration by consumers.
