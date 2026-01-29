## 2024-05-23 - Async Timeout Optimization
**Learning:** `Task.WhenAny` combined with `Task.Delay` for timeouts creates "zombie" timers that persist until the delay expires, even if the primary task completes early. This can lead to resource leaks in high-throughput scenarios.
**Action:** In .NET 6+, always prefer `Task.WaitAsync(TimeSpan)` which handles cancellation and timer disposal efficiently. When refactoring legacy code, ensure exception handling behavior is preserved (e.g., catching `TimeoutException` if the original code just checked a flag after `WhenAny`).
