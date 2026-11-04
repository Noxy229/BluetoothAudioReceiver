using BluetoothAudioReceiver.Desktop.Services;
using BluetoothAudioReceiver.Services;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine($"PASS {message}");
}

var devices = LinuxReceiver.ParseDevices("[NEW] Controller XX\nDevice aa:bb:cc:dd:ee:ff My Phone\nDevice AA:BB:CC:DD:EE:FF Duplicate\nDevice bad Ignore\nDevice 11:22:33:44:55:66 Tablet\n");
Check(devices.Count == 2 && devices[0].Name == "My Phone", "Paired-device parsing ignores noise and deduplicates addresses");
const string sources = """
[
 {"name":"alsa_input.pci.microphone"},
 {"name":"bluez_input.AA_BB_CC_DD_EE_FF.monitor"},
 {"name":"bluez_input.11_22_33_44_55_66.0"},
 {"name":"bluez_input.AA_BB_CC_DD_EE_FF.0"}
]
""";
Check(LinuxReceiver.FindBluetoothSource(sources, "AA:BB:CC:DD:EE:FF") == "bluez_input.AA_BB_CC_DD_EE_FF.0", "Routing selects this phone, never a microphone, monitor or another phone");
Check(LinuxReceiver.FindBluetoothSource(sources, "00:00:00:00:00:00") == null, "Missing Bluetooth audio never falls back to the microphone");
Check(LinuxReceiver.FindBluetoothSource("[{\"name\":\"bluez_source.aa_bb_cc_dd_ee_ff.a2dp_source\"}]", "AA:BB:CC:DD:EE:FF") != null, "PulseAudio source naming is supported");

var commands = new FakeCommands();
var receiver = new LinuxReceiver(commands);
commands.Results.Enqueue(sources); commands.Results.Enqueue("42\n");
await receiver.RouteAsync(devices[0]);
await receiver.RouteAsync(devices[0]);
Check(commands.Calls.Count == 2 && receiver.IsRouting, "Repeated play does not create duplicate loopbacks");
commands.Results.Enqueue(""); await receiver.StopAsync(); await receiver.StopAsync();
Check(commands.Calls[^1].SequenceEqual(new[] { "pactl", "unload-module", "42" }) && !receiver.IsRouting, "Stop unloads only the module owned by this app");
commands.Results.Enqueue("[]");
try { await receiver.RouteAsync(devices[0]); throw new Exception("Expected missing source error"); }
catch (InvalidOperationException) { Check(!receiver.IsRouting, "Missing input leaves routing stopped"); }
commands.Results.Enqueue("Failed to connect: org.bluez.Error.Failed");
try { await receiver.ConnectAsync(devices[0]); throw new Exception("Expected connection error"); }
catch (InvalidOperationException) { Check(true, "BlueZ failures are detected even with exit status zero"); }
commands.Results.Enqueue(sources); commands.Results.Enqueue("42"); await receiver.RouteAsync(devices[0]);
commands.FailNext = true;
try { await receiver.StopAsync(); throw new Exception("Expected unload error"); }
catch (InvalidOperationException) { Check(receiver.IsRouting, "Failed cleanup retains the module ID so the user can retry"); }
commands.Results.Enqueue(""); await receiver.StopAsync();

var runner = new CommandRunner();
if (OperatingSystem.IsLinux())
{
    Check((await runner.RunAsync("/usr/bin/printf", "%s", "a b;$(false)")).Trim() == "a b;$(false)", "Process arguments remain literal without shell interpretation");
    try { await runner.RunAsync("/bin/false"); throw new Exception("Expected exit error"); }
    catch (InvalidOperationException) { Check(true, "Nonzero command exits fail visibly"); }
}
var loc = LocalizationService.Instance;
loc.CurrentLanguage = "it";
Check(loc["Scanning"] == "Scanning for devices...", "Incomplete translations fall back to English");
loc.CurrentLanguage = "de";
Check(loc["Connected"] == "Verbunden", "German translation loads");
loc.CurrentLanguage = "invalid";
Check(loc.CurrentLanguage == "de", "Unknown language preserves the current selection");
var failedStart = new Windows.Media.Audio.AudioPlaybackConnection { ThrowOnStart = true };
var failedOpen = new Windows.Media.Audio.AudioPlaybackConnection { FailOpen = true };
var success = new Windows.Media.Audio.AudioPlaybackConnection();
Windows.Media.Audio.AudioPlaybackConnection.Pending.Enqueue(failedStart);
Windows.Media.Audio.AudioPlaybackConnection.Pending.Enqueue(failedOpen);
Windows.Media.Audio.AudioPlaybackConnection.Pending.Enqueue(success);
using var audio = new AudioService();
var states = new List<string>();
audio.ConnectionStateChanged += (_, connected) => states.Add(connected ? "connected" : "disconnected");
audio.StreamingStateChanged += (_, state) => states.Add(state);
Check(await audio.OpenConnectionAsync("phone"), "Windows connection recovers after failed start and failed open");
Check(failedStart.DisposeCount == 1 && failedOpen.DisposeCount == 1, "Every failed Windows connection is disposed exactly once");
Check(audio.IsConnected && audio.CurrentDeviceId == "phone", "Windows connection reports the active device");
Check(states[^2] == "connected" && states[^1] == "Streaming", "Connected notification does not overwrite the streaming status");
success.RemoteClose();
Check(!audio.IsConnected && !audio.IsStreaming && audio.CurrentDeviceId == null && states[^1] == "disconnected", "Remote disconnection clears Windows connection state");
await audio.CloseConnectionAsync();
audio.Dispose(); audio.Dispose();
Check(success.DisposeCount == 1, "Repeated close and disposal do not release the connection twice");
Console.WriteLine("All checks passed.");

sealed class FakeCommands : ICommandRunner
{
    public Queue<string> Results { get; } = new();
    public List<string[]> Calls { get; } = new();
    public bool FailNext { get; set; }
    public Task<string> RunAsync(string executable, params string[] arguments)
    {
        Calls.Add(new[] { executable }.Concat(arguments).ToArray());
        if (FailNext) { FailNext = false; throw new InvalidOperationException("Simulated failure"); }
        return Task.FromResult(Results.Dequeue());
    }
}
