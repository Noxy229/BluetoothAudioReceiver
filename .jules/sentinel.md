## 2024-10-27 - Committed Private Keys
**Vulnerability:** Found `scripts/TestCert.pfx` committed to the repository. This is a PKCS#12 certificate file containing a private key, protected by a weak default password ('password') hardcoded in scripts.
**Learning:** Developers often commit 'test' certificates to simplify the build/sign process for others, overlooking that anyone can use this key to sign malicious code that appears to come from the same entity (if the cert is trusted).
**Prevention:** Add `*.pfx` and `*.p12` to `.gitignore`. Use CI/CD secrets for real signing keys, and generate ephemeral self-signed certs on the fly for test builds.

## 2024-10-27 - Secure Crash Logging
**Vulnerability:** The application was displaying raw stack traces in the crash dialog (Information Disclosure) and appending to the log file indefinitely (Potential Resource Exhaustion/DoS).
**Learning:** Default global exception handlers often expose too much internal information to users. Unbounded log files can consume all available disk space if a crash loop occurs.
**Prevention:** Implement log rotation (e.g., max 5MB, keep 1 backup) and display sanitized, generic error messages to the user while pointing them to the secure log file location.

## 2024-10-27 - Bluetooth Device Name Sanitization
**Vulnerability:** Bluetooth device names were used directly from discovery data without sanitization. Malicious devices could advertise names containing control characters (log injection/terminal corruption) or excessive length (DoS/UI breaking).
**Learning:** External hardware inputs are untrusted data sources. Just because a device is "paired" or "local" doesn't mean its metadata is safe to render or log directly.
**Prevention:** Implemented `SanitizeDeviceName` in `BluetoothService` to strip control characters and enforce length limits before any processing or display.
