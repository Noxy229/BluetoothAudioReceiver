using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using BluetoothAudioReceiver.Desktop.Services;

namespace BluetoothAudioReceiver.Desktop;

public sealed class MainWindow : Window
{
    private readonly LinuxReceiver _receiver = new(new CommandRunner());
    private readonly ComboBox _devices = new() { HorizontalAlignment = HorizontalAlignment.Stretch, PlaceholderText = "Choose a paired phone" };
    private readonly TextBlock _status = new() { Text = "Ready", FontSize = 15, TextWrapping = TextWrapping.Wrap };
    private readonly TextBlock _detail = new() { TextWrapping = TextWrapping.Wrap, Foreground = Brush.Parse("#B5BAC4") };
    private readonly Button _refresh = new() { Content = "Refresh devices" };
    private readonly Button _connect = new() { Content = "Connect" };
    private readonly Button _route = new() { Content = "Play through speakers" };
    private readonly Button _stop = new() { Content = "Stop playback", IsEnabled = false };
    private bool _busy;
    private bool _allowClose;

    public MainWindow()
    {
        Title = "Bluetooth Audio Receiver";
        Width = 560; Height = 500; MinWidth = 420; MinHeight = 400;
        Background = Brush.Parse("#17191D");
        var layout = new StackPanel { Margin = new Thickness(28), Spacing = 18 };
        layout.Children.Add(new TextBlock { Text = "Audio Receiver", FontSize = 26, FontWeight = FontWeight.SemiBold });
        layout.Children.Add(new TextBlock { Text = OperatingSystem.IsMacOS() ? "macOS · AirPlay setup" : "Linux · Bluetooth audio", Foreground = Brush.Parse("#B5BAC4") });
        if (OperatingSystem.IsLinux()) BuildLinux(layout);
        else BuildMac(layout);
        layout.Children.Add(new Separator());
        layout.Children.Add(_status);
        layout.Children.Add(_detail);
        Content = new ScrollViewer { Content = layout };
        Closing += OnClosing;
    }

    private void BuildLinux(StackPanel layout)
    {
        layout.Children.Add(new TextBlock { Text = "Pair your phone in the system Bluetooth settings first. Connect it here, start music on your phone, then route the audio to your speakers.", TextWrapping = TextWrapping.Wrap });
        layout.Children.Add(_devices);
        var buttons = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var button in new[] { _refresh, _connect, _route, _stop })
        {
            button.Margin = new Thickness(0, 0, 8, 8);
            buttons.Children.Add(button);
        }
        layout.Children.Add(buttons);
        var guide = new Button { Content = "Linux setup guide" };
        guide.Click += (_, _) => Open("https://github.com/Noxy229/BluetoothAudioReceiver/blob/master/docs/linux.md");
        layout.Children.Add(guide);
        _refresh.Click += async (_, _) => await RunAsync(async () =>
        {
            _devices.ItemsSource = await _receiver.GetDevicesAsync();
            _status.Text = "Choose your phone";
            _detail.Text = "Only devices already paired with this computer are listed.";
        });
        _connect.Click += async (_, _) => await RunAsync(async () =>
        {
            await _receiver.ConnectAsync(SelectedDevice());
            _status.Text = "Bluetooth connection requested";
            _detail.Text = "Start music on your phone, then choose ‘Play through speakers’.";
        });
        _route.Click += async (_, _) => await RunAsync(async () =>
        {
            await _receiver.RouteAsync(SelectedDevice());
            _status.Text = "Audio routed to your default speakers";
            _detail.Text = "Stop playback here to remove the audio route. This does not unpair your phone.";
        });
        _stop.Click += async (_, _) => await RunAsync(async () =>
        {
            await _receiver.StopAsync();
            _status.Text = "Playback stopped";
            _detail.Text = "Your phone remains paired.";
        });
        Opened += async (_, _) => await RunAsync(async () => _devices.ItemsSource = await _receiver.GetDevicesAsync());
        _devices.SelectionChanged += (_, _) => UpdateButtons();
        UpdateButtons();
    }

    private void BuildMac(StackPanel layout)
    {
        layout.Children.Add(new TextBlock { Text = "Use your Mac as an AirPlay speaker", FontSize = 18 });
        layout.Children.Add(new TextBlock
        {
            Text = "This Mac edition helps you set up Apple's built-in AirPlay Receiver. It does not receive Bluetooth audio.\n\n1. Open System Settings → General → AirDrop & Handoff (or AirDrop & Continuity).\n2. Turn on AirPlay Receiver and choose who may connect.\n3. On your iPhone or iPad, open the audio output picker and select this Mac.\n\nUse the same Wi-Fi network. AirPlay Receiver requires a supported Mac and macOS version.",
            TextWrapping = TextWrapping.Wrap
        });
        var settings = new Button { Content = "Open System Settings" };
        settings.Click += (_, _) => Open("x-apple.systempreferences:com.apple.AirDrop-Handoff-Settings.extension");
        var help = new Button { Content = "Apple's AirPlay guide" };
        help.Click += (_, _) => Open("https://support.apple.com/guide/mac-help/mchld7e543a0/mac");
        layout.Children.Add(settings); layout.Children.Add(help);
        _status.Text = "Playback is managed by macOS";
        _detail.Text = "No background service or system configuration is installed by this app.";
    }

    private PairedDevice SelectedDevice() => _devices.SelectedItem as PairedDevice
        ?? throw new InvalidOperationException("Choose a paired phone first.");

    private async Task RunAsync(Func<Task> action)
    {
        if (_busy) return;
        _busy = true; UpdateButtons();
        _status.Text = "Working…"; _detail.Text = "";
        try { await action(); }
        catch (Win32Exception) { _status.Text = "A required tool is missing"; _detail.Text = "Install BlueZ (bluetoothctl) and PulseAudio tools (pactl). See the Linux setup guide for your distribution."; }
        catch (Exception ex) { _status.Text = "Could not complete the request"; _detail.Text = ex.Message; }
        finally { _busy = false; UpdateButtons(); }
    }

    private void UpdateButtons()
    {
        _refresh.IsEnabled = !_busy;
        _devices.IsEnabled = !_busy && !_receiver.IsRouting;
        _connect.IsEnabled = !_busy && _devices.SelectedItem != null && !_receiver.IsRouting;
        _route.IsEnabled = !_busy && _devices.SelectedItem != null && !_receiver.IsRouting;
        _stop.IsEnabled = !_busy && _receiver.IsRouting;
    }

    private void Open(string target)
    {
        try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); }
        catch (Exception ex) { _status.Text = "Could not open the link"; _detail.Text = ex.Message; }
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose || !OperatingSystem.IsLinux()) return;
        e.Cancel = true;
        if (_busy) { _detail.Text = "Wait for the current operation to finish before closing."; return; }
        await RunAsync(async () =>
        {
            await _receiver.StopAsync();
            _allowClose = true;
        });
        if (_allowClose) Close();
    }
}
