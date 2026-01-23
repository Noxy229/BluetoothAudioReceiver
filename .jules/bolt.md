## 2024-05-22 - Windows-specific dependencies
**Learning:** This codebase relies on `Windows.Devices.Enumeration` and other Windows-specific APIs, making it impossible to compile or run tests on Linux environments.
**Action:** Verify logic through strict code analysis and ensure valid syntax when modifying files, as the compiler is unavailable.
