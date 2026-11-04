using System.Text.Json;
using System.Text.RegularExpressions;

namespace BluetoothAudioReceiver.Desktop.Services;

public sealed record PairedDevice(string Address, string Name)
{
    public override string ToString() => $"{Name}  ·  {Address}";
}

public sealed class LinuxReceiver(ICommandRunner commands)
{
    private string? _loopbackId;
    public bool IsRouting => _loopbackId != null;

    public async Task<IReadOnlyList<PairedDevice>> GetDevicesAsync()
    {
        await commands.RunAsync("pactl", "info");
        var output = await commands.RunAsync("bluetoothctl", "devices", "Paired");
        if (output.Contains("Invalid command", StringComparison.OrdinalIgnoreCase))
            output = await commands.RunAsync("bluetoothctl", "paired-devices");
        return ParseDevices(output);
    }

    public static IReadOnlyList<PairedDevice> ParseDevices(string output) =>
        output.Split('\n').Select(line => Regex.Match(line.Trim(),
            @"^Device ([0-9A-Fa-f]{2}(?::[0-9A-Fa-f]{2}){5}) (.+)$"))
            .Where(match => match.Success)
            .Select(match => new PairedDevice(match.Groups[1].Value.ToUpperInvariant(), match.Groups[2].Value))
            .DistinctBy(device => device.Address).OrderBy(device => device.Name).ToArray();

    public async Task ConnectAsync(PairedDevice device)
    {
        if (!Regex.IsMatch(device.Address, @"\A[0-9A-Fa-f]{2}(?::[0-9A-Fa-f]{2}){5}\z"))
            throw new ArgumentException("Invalid Bluetooth address.", nameof(device));
        await StopAsync();
        var result = await commands.RunAsync("bluetoothctl", "--timeout", "20", "connect", device.Address);
        // Some BlueZ versions report a failed command with exit status 0.
        if (result.Contains("Failed", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(result.Trim());
    }

    public async Task RouteAsync(PairedDevice device)
    {
        if (IsRouting) return;
        var sources = await commands.RunAsync("pactl", "--format=json", "list", "sources");
        var source = FindBluetoothSource(sources, device.Address);
        if (source == null)
            throw new InvalidOperationException("No audio input from this device yet. Start playback on your phone, then choose ‘Play through speakers’. Check the Linux setup guide if it still does not appear.");
        var output = await commands.RunAsync("pactl", "load-module", "module-loopback",
            $"source={source}", "sink=@DEFAULT_SINK@", "latency_msec=100");
        if (!uint.TryParse(output.Trim(), out var id))
            throw new InvalidOperationException("The audio server did not return a loopback module ID.");
        _loopbackId = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string? FindBluetoothSource(string json, string address)
    {
        var normalized = address.Replace(':', '_');
        using var document = JsonDocument.Parse(json);
        foreach (var source in document.RootElement.EnumerateArray())
        {
            if (!source.TryGetProperty("name", out var nameProperty)) continue;
            var name = nameProperty.GetString();
            if (name == null || name.EndsWith(".monitor", StringComparison.Ordinal)) continue;
            if ((name.StartsWith("bluez_input.", StringComparison.Ordinal) ||
                 name.StartsWith("bluez_source.", StringComparison.Ordinal)) &&
                name.Contains(normalized, StringComparison.OrdinalIgnoreCase)) return name;
        }
        return null;
    }

    public async Task StopAsync()
    {
        if (_loopbackId == null) return;
        await commands.RunAsync("pactl", "unload-module", _loopbackId);
        _loopbackId = null;
    }
}
