# Sentinel Security Journal

## 2024-05-22 - Secure Crash Logging
**Vulnerability:** Application logs crash details (including stack traces) to `crash_log.txt` on the user's Desktop.
**Learning:** Writing sensitive debug information to a highly visible and accessible location like the Desktop increases the risk of information leakage and is poor UX. It bypasses standard OS conventions for application data.
**Prevention:** Use `Environment.SpecialFolder.LocalApplicationData` for application logs. This keeps logs hidden from casual view but accessible for support, and respects Windows directory standards.
