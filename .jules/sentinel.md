## 2024-10-27 - Committed Private Keys
**Vulnerability:** Found `scripts/TestCert.pfx` committed to the repository. This is a PKCS#12 certificate file containing a private key, protected by a weak default password ('password') hardcoded in scripts.
**Learning:** Developers often commit 'test' certificates to simplify the build/sign process for others, overlooking that anyone can use this key to sign malicious code that appears to come from the same entity (if the cert is trusted).
**Prevention:** Add `*.pfx` and `*.p12` to `.gitignore`. Use CI/CD secrets for real signing keys, and generate ephemeral self-signed certs on the fly for test builds.

## 2024-10-27 - Secure Crash Logging
**Vulnerability:** The application was displaying raw stack traces in the crash dialog (Information Disclosure) and appending to the log file indefinitely (Potential Resource Exhaustion/DoS).
**Learning:** Default global exception handlers often expose too much internal information to users. Unbounded log files can consume all available disk space if a crash loop occurs.
**Prevention:** Implement log rotation (e.g., max 5MB, keep 1 backup) and display sanitized, generic error messages to the user while pointing them to the secure log file location.

## 2024-10-27 - Unsanitized Bluetooth Inputs
**Vulnerability:** `BluetoothService` trusted `DeviceInformation.Name` directly from the OS enumeration, allowing control characters and potentially long strings to enter the application model.
**Learning:** Even hardware enumeration APIs return "user input" (from the remote device owner) and must be treated as untrusted.
**Prevention:** Always sanitize external strings (trim, remove control chars, limit length) before crossing the boundary into the application model.
