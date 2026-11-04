#!/usr/bin/env bash
set -euo pipefail
runtime="${1:?runtime required}"
version="${2:?version required}"
publish="${3:?publish directory required}"
out="${4:?output directory required}"
root=$(mktemp -d)
trap 'rm -rf "$root"' EXIT
app="$root/Audio Receiver.app"
mkdir -p "$app/Contents/MacOS" "$out"
cp -R "$publish/." "$app/Contents/MacOS/"
chmod +x "$app/Contents/MacOS/BluetoothAudioReceiver"
cat > "$app/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
<key>CFBundleExecutable</key><string>BluetoothAudioReceiver</string>
<key>CFBundleIdentifier</key><string>io.github.noxy229.audio-receiver</string>
<key>CFBundleName</key><string>Audio Receiver</string>
<key>CFBundlePackageType</key><string>APPL</string>
<key>CFBundleShortVersionString</key><string>$version</string>
<key>CFBundleVersion</key><string>$version</string>
<key>LSMinimumSystemVersion</key><string>12.0</string>
<key>NSHighResolutionCapable</key><true/>
</dict></plist>
PLIST
codesign --force --deep --sign - "$app"
ditto -c -k --sequesterRsrc --keepParent "$app" "$out/BluetoothAudioReceiver-$runtime-AirPlay-Setup.zip"
