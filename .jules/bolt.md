## 2024-05-22 - Windows-specific dependencies
**Learning:** This codebase relies on `Windows.Devices.Enumeration` and other Windows-specific APIs, making it impossible to compile or run tests on Linux environments.
**Action:** Verify logic through strict code analysis and ensure valid syntax when modifying files, as the compiler is unavailable.

## 2024-05-22 - Lazy Loading Localization
**Learning:** The `LocalizationService` was initializing all 15 language dictionaries at startup using a static initializer. This front-loaded memory allocation and CPU time.
**Action:** Refactored to lazy-load dictionaries on demand. Used a cache to prevent re-allocation. This pattern is useful for any resource-heavy static data that is rarely accessed in its entirety.

## 2026-01-25 - Redundant Property Updates in MVVM
**Learning:** In a shared-object architecture (Service passes reference to ViewModel), explicit event handlers in the ViewModel that copy properties from the Service object to the ViewModel object are O(N) redundant operations if the object implements `INotifyPropertyChanged`.
**Action:** Trust data binding. If the Service updates the object, the UI receives the `PropertyChanged` event directly. Remove the redundant ViewModel event handler.

## 2026-01-25 - DeviceWatcher Enumeration Flood
**Learning:** `DeviceWatcher` fires `Added` events rapidly for cached devices on startup. Updating UI status strings for every single event causes visible jitter/overhead.
**Action:** Use `EnumerationCompleted` event to batch the status update until the initial flood is over.

## 2026-01-25 - Efficient Async Timeouts
**Learning:** `Task.WhenAny(task, Task.Delay(timeout))` leaves the timer task running even if the primary task completes immediately, wasting system timer resources.
**Action:** Use `task.WaitAsync(timeout)` in .NET 6+ environments. It handles the timeout internally and cancels the timer mechanism upon completion, but remember to catch `TimeoutException`.
