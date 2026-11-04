using System.Diagnostics;

namespace BluetoothAudioReceiver.Desktop.Services;

public interface ICommandRunner
{
    Task<string> RunAsync(string executable, params string[] arguments);
}

public sealed class CommandRunner : ICommandRunner
{
    public async Task<string> RunAsync(string executable, params string[] arguments)
    {
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        start.Environment["LC_ALL"] = "C";
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException($"Cannot start {executable}.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(25));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException($"{executable} did not respond within 25 seconds.");
        }
        var stdout = await output;
        var stderr = await error;
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{executable}: {(string.IsNullOrWhiteSpace(stderr) ? stdout : stderr).Trim()}");
        return stdout;
    }
}
