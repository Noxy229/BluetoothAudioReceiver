## 2024-05-22 - Windows-specific dependencies
**Learning:** This codebase relies on `Windows.Devices.Enumeration` and other Windows-specific APIs, making it impossible to compile or run tests on Linux environments.
**Action:** Verify logic through strict code analysis and ensure valid syntax when modifying files, as the compiler is unavailable.

## 2024-05-22 - Lazy Loading Localization
**Learning:** The `LocalizationService` was initializing all 15 language dictionaries at startup using a static initializer. This front-loaded memory allocation and CPU time.
**Action:** Refactored to lazy-load dictionaries on demand. Used a cache to prevent re-allocation. This pattern is useful for any resource-heavy static data that is rarely accessed in its entirety.

## 2026-01-25 - Redundant Property Updates in MVVM
**Learning:** In a shared-object architecture (Service passes reference to ViewModel), explicit event handlers in the ViewModel that copy properties from the Service object to the ViewModel object are O(N) redundant operations if the object implements `INotifyPropertyChanged`.
**Action:** Trust data binding. If the Service updates the object, the UI receives the `PropertyChanged` event directly. Remove the redundant ViewModel event handler.
