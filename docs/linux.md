# Linux setup

The Linux edition is a preview. It controls BlueZ through `bluetoothctl` and creates a loopback from the selected phone's Bluetooth audio input to the default output using `pactl`. Automated tests cover parsing, device selection and route cleanup. Physical Bluetooth reception has not yet been validated across these distributions.

## Compatibility

Use an x64 or ARM64 desktop with glibc 2.35 or newer (the release build baseline), X11 or an XWayland session, BlueZ and a running per-user audio server. PipeWire needs its PulseAudio compatibility service and Bluetooth support; PulseAudio needs its Bluetooth module. The audio server must expose an A2DP receiver role. No root access is needed to run the app.

| Distribution family | Package | Notes |
| --- | --- | --- |
| Debian 12+, Ubuntu 22.04+, Linux Mint 21+ | `.deb` | Install BlueZ, `pulseaudio-utils` and Bluetooth support for your audio server. |
| Fedora | `.rpm` | Use PipeWire, `pipewire-pulseaudio`, WirePlumber, BlueZ and `pulseaudio-utils`. |
| Arch / EndeavourOS / Manjaro | `.tar.gz` | Install BlueZ, `bluez-utils`, `libpulse`, PipeWire and WirePlumber. |
| openSUSE | `.tar.gz` or `.rpm` | Portable archive avoids differences in distribution package names. |
| Other glibc distributions | `.tar.gz` | Install the equivalent runtime tools and GUI libraries yourself. |
| Alpine / other musl distributions | Not provided | The glibc binaries do not run natively on musl. |
| Headless servers | Not provided | This edition needs a desktop session and a user audio server. |

These are packaging targets and prerequisites, not a hardware-tested distribution certification list. There are no 32-bit x86 or ARMv7 builds.

## Install

### Debian / Ubuntu / Mint

```bash
sudo apt install bluez pulseaudio-utils libx11-6 libice6 libsm6 libfontconfig1
# PipeWire users also need this Bluetooth plugin:
sudo apt install libspa-0.2-bluetooth
sudo apt install ./BluetoothAudioReceiver-linux-x64.deb
```

Use the ARM64 package on an ARM64 system. If your desktop uses PulseAudio instead of PipeWire, install `pulseaudio-module-bluetooth` rather than switching audio servers.

### Fedora

```bash
sudo dnf install bluez pulseaudio-utils pipewire-pulseaudio wireplumber
sudo dnf install ./BluetoothAudioReceiver-linux-x64.rpm
```

### Arch family

```bash
sudo pacman -S bluez bluez-utils libpulse pipewire pipewire-pulse wireplumber libx11 libice libsm fontconfig
```

Do not install a second competing audio server. If your system already uses PulseAudio, keep it and install its Bluetooth support.

### Portable archive

```bash
mkdir -p ~/Applications/audio-receiver
tar -xzf BluetoothAudioReceiver-linux-x64.tar.gz -C ~/Applications/audio-receiver
~/Applications/audio-receiver/BluetoothAudioReceiver
```

The archive includes .NET but not system audio/Bluetooth services or display libraries. It can also be used on immutable distributions when extracted into a writable user directory, provided the host has the required services. Flatpak, Snap and AppImage packages are not included.

## No audio

1. Pair your phone in the desktop's Bluetooth settings. Make sure both devices allow an audio connection.
2. Check that `bluetoothctl show` reports an available, powered adapter and that `pactl info` reaches your user audio server. Run the app as your normal desktop user.
3. Connect the phone and start music. `pactl --format=json list sources` should show a `bluez_input` or `bluez_source` entry containing that phone's Bluetooth address.
4. Click **Play through speakers**. The app intentionally refuses to substitute a microphone or another phone's audio input.
5. If no Bluetooth source appears, check your audio server's Bluetooth support and receiver roles. See the [WirePlumber Bluetooth configuration](https://pipewire.pages.freedesktop.org/wireplumber/daemon/configuration/bluetooth.html) and [PipeWire Bluetooth properties](https://pipewire.pages.freedesktop.org/pipewire/devel/page_man_pipewire-props_7.html). Configure the appropriate version for your distribution; the app does not overwrite these files.

Routing uses the audio server's [module-loopback](https://pipewire.pages.freedesktop.org/pipewire/page_pulse_module_loopback.html). Stop playback or close the window to remove the module. If cleanup fails, the app stays open and shows the error so you can retry. A forced kill or crash may leave the loopback loaded until the audio server restarts; inspect `pactl list short modules` and unload only the relevant module with `pactl unload-module ID`.

Pairing and Bluetooth connection state remain managed by BlueZ. Stopping playback removes the local speaker route; it does not disconnect or unpair the phone.
