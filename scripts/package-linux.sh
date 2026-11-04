#!/usr/bin/env bash
set -euo pipefail
runtime="${1:?runtime required}"
version="${2:?version required}"
publish="${3:?publish directory required}"
out="${4:?output directory required}"
case "$runtime" in
  linux-x64) deb_arch=amd64; rpm_arch=x86_64 ;;
  linux-arm64) deb_arch=arm64; rpm_arch=aarch64 ;;
  *) echo "Unsupported runtime: $runtime" >&2; exit 1 ;;
esac
mkdir -p "$out"
root=$(mktemp -d)
trap 'rm -rf "$root"' EXIT
mkdir -p "$root/opt/bluetooth-audio-receiver" "$root/usr/share/applications" "$root/usr/bin"
cp -a "$publish/." "$root/opt/bluetooth-audio-receiver/"
cp packaging/bluetooth-audio-receiver.desktop "$root/usr/share/applications/"
ln -s /opt/bluetooth-audio-receiver/BluetoothAudioReceiver "$root/usr/bin/bluetooth-audio-receiver"
chmod +x "$root/opt/bluetooth-audio-receiver/BluetoothAudioReceiver"
fpm -s dir -t deb -n bluetooth-audio-receiver -v "$version" -a "$deb_arch" \
  --description 'Receive Bluetooth audio through BlueZ and PipeWire or PulseAudio' \
  --license MIT --url https://github.com/Noxy229/BluetoothAudioReceiver \
  --depends bluez --depends pulseaudio-utils --depends libx11-6 --depends libice6 --depends libsm6 --depends libfontconfig1 \
  -C "$root" -p "$out/BluetoothAudioReceiver-$runtime.deb" .
fpm -s dir -t rpm -n bluetooth-audio-receiver -v "$version" -a "$rpm_arch" \
  --description 'Receive Bluetooth audio through BlueZ and PipeWire or PulseAudio' \
  --license MIT --url https://github.com/Noxy229/BluetoothAudioReceiver \
  --depends bluez --depends pulseaudio-utils --depends libX11 --depends libICE --depends libSM --depends fontconfig \
  -C "$root" -p "$out/BluetoothAudioReceiver-$runtime.rpm" .
tar -C "$publish" -czf "$out/BluetoothAudioReceiver-$runtime.tar.gz" .
