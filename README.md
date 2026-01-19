# Bluetooth Audio Receiver

## ✨ Features
- 🎵 Bluetooth A2DP Sink - Receive audio from any Bluetooth device
- 🌙 AMOLED Dark Theme - Pure black design, easy on the eyes
- 📌 System Tray - Runs quietly in the background
- ⚡ Low Overhead - Event-driven, no polling, minimal resource usage
- 🚀 Autostart - Optional startup with Windows (minimized mode available)
- ❓ Built-in Help - Troubleshooting guide for common issues

## 🔧 Requirements
- Windows 10 (Version 2004+) or Windows 11
- Bluetooth adapter with A2DP support
- A paired Bluetooth device (phone, tablet, etc.)

## 📥 Installation
1. Download the latest release from [Releases](https://github.com/Noxy229/BluetoothAudioReceiver/releases)
2. Run BluetoothAudioReceiver.exe
3. Pair your phone via Windows Bluetooth settings
4. Select your device and click "Verbinden"

> [!NOTE]
> **SmartScreen Warning**: You may see a "Windows protected your PC" popup because the app is currently waiting for Microsoft's verification.
> **Status**: We have set up the application with Microsoft and are waiting for the "Unknown Publisher" warning to be resolved.
> In the meantime, click **"More info" → "Run anyway"** to start the app.

## 🛠️ Building from Source
```
# Clone the repository
git clone https://github.com/Noxy/BluetoothAudioReceiver.git

# Navigate to project
cd BluetoothAudioReceiver

# Build
dotnet build

# Run
dotnet run

# Publish standalone exe
dotnet publish -c Release --self-contained true -p:PublishSingleFile=true
```

## Signing Scripts (New)
See the `scripts/` directory for code signing utilities:
- `Create-Test-Cert.ps1`: Generates a local self-signed certificate for testing.
- `Sign-Build.ps1`: Builds and signs the application using a PFX certificate.
