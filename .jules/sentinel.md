## 2024-10-27 - Committed Private Keys
**Vulnerability:** Found `scripts/TestCert.pfx` committed to the repository. This is a PKCS#12 certificate file containing a private key, protected by a weak default password ('password') hardcoded in scripts.
**Learning:** Developers often commit 'test' certificates to simplify the build/sign process for others, overlooking that anyone can use this key to sign malicious code that appears to come from the same entity (if the cert is trusted).
**Prevention:** Add `*.pfx` and `*.p12` to `.gitignore`. Use CI/CD secrets for real signing keys, and generate ephemeral self-signed certs on the fly for test builds.

## 2024-10-27 - Secure Crash Logging
**Vulnerability:** The application was displaying raw stack traces in the crash dialog (Information Disclosure) and appending to the log file indefinitely (Potential Resource Exhaustion/DoS).
**Learning:** Default global exception handlers often expose too much internal information to users. Unbounded log files can consume all available disk space if a crash loop occurs.
**Prevention:** Implement log rotation (e.g., max 5MB, keep 1 backup) and display sanitized, generic error messages to the user while pointing them to the secure log file location.

## 2026-01-29 - Untrusted Hardware Inputs
**Vulnerability:** Bluetooth device names were displayed directly from the `DeviceWatcher` API without validation. Malicious devices could broadcast names with control characters (log spoofing) or excessive length (UI DoS/layout breaking).
**Learning:** Even "trusted" hardware APIs return data that originates from untrusted external sources (the airwaves). Desktop apps are not immune to injection attacks via these channels.
**Prevention:** Sanitize all external strings. Remove control characters (ASCII 0-31), trim whitespace, and enforce reasonable length limits before passing data to the UI or logs.
