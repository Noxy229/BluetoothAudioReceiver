## Windows

- Release failed audio connections before retrying, and clear the connection state when the phone disconnects.
- Keep Bluetooth callbacks and tray updates on the UI thread.
- Clear stale device entries when refreshing; prevent selection changes during an active connection.
- Connect automatically to the saved device when the setting is enabled.
- Apply the saved language at startup and localize the main connection controls.
- Restore settings when the settings window is dismissed, including through the window close button.
- Release native tray icon resources on exit and limit tooltip text to the supported length.
- Preserve the shared notification settings after saving, improve secondary text contrast, and show the actual app version in Help.
- Save settings through a temporary file to reduce the risk of a truncated settings file.

## Additional editions

- **Linux preview:** BlueZ connection controls and speaker routing through PipeWire/PulseAudio. Packages for x64 and ARM64: DEB, RPM and portable tar.gz. Requires a configured desktop Bluetooth/audio stack; real-device testing is still needed.
- **macOS AirPlay setup companion:** Intel and Apple Silicon app bundles with setup instructions and links to Apple's built-in AirPlay Receiver. This edition does not implement Bluetooth reception.

## Releases and documentation

- Build all editions and run regression checks before publishing a tagged release.
- Include SHA-256 checksums. Remove automatic bot merges and per-build test-certificate signing.
- Rewrite the README with platform downloads, setup instructions and explicit support limits.

Windows downloads are unsigned; Mac bundles are ad-hoc signed and not notarized. Linux preview and macOS setup UI have build/automated checks, not end-to-end hardware validation.
