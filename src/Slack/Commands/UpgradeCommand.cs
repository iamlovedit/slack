using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO.Compression;
using Slack.Localization;
using Slack.Serialization;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace Slack.Commands;

public class UpgradeCommand : ICommand
{
    public string Name => "upgrade";
    public string Description => Strings.UpgradeDescription;
    public string[] Aliases => [];

    private const string Repo = "iamlovedit/slack";
    private const string BinaryName = "slack";

    public int Execute(string[] args)
    {
        return ExecuteAsync().GetAwaiter().GetResult();
    }

    private async Task<int> ExecuteAsync()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[cyan]🔄 Slack Upgrader[/]");
        AnsiConsole.MarkupLine("[dim]==================[/]");
        AnsiConsole.WriteLine();

        try
        {
            // Get current version
            var currentVersion = GetCurrentVersion();
            AnsiConsole.MarkupLine($"[blue][[INFO]][/] {Strings.UpgradeCurrentVersion}: [cyan]{currentVersion}[/]");

            // Detect platform
            var platform = DetectPlatform();
            AnsiConsole.MarkupLine($"[blue][[INFO]][/] {Strings.UpgradePlatform}: [cyan]{platform}[/]");

            // Get latest version from GitHub
            AnsiConsole.MarkupLine($"[blue][[INFO]][/] {Strings.UpgradeCheckingLatest}");
            var latestVersion = await GetLatestVersionAsync();
            AnsiConsole.MarkupLine($"[blue][[INFO]][/] {Strings.UpgradeLatestVersion}: [cyan]{latestVersion}[/]");

            // Compare versions
            if (!NeedsUpgrade(currentVersion, latestVersion))
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[green]✓[/] {Strings.Format(Strings.UpgradeAlreadyLatest, currentVersion)}");
                return 0;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]→[/] {Strings.Format(Strings.UpgradeAvailable, currentVersion, latestVersion)}");
            AnsiConsole.WriteLine();

            // Confirm upgrade
            if (!AnsiConsole.Confirm(Strings.UpgradeConfirm))
            {
                AnsiConsole.MarkupLine($"[yellow]{Strings.UpgradeCancelled}[/]");
                return 0;
            }

            AnsiConsole.WriteLine();

            // Download and install
            await DownloadAndInstallAsync(platform, latestVersion);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[green]✓[/] {Strings.Format(Strings.UpgradeSuccess, currentVersion, latestVersion)}");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[dim]{Strings.UpgradeRestartHint}[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][[ERROR]][/] {Strings.Format(Strings.UpgradeFailed, ex.Message)}");
            return 1;
        }
    }

    private static string GetCurrentVersion()
    {
        var version = typeof(UpgradeCommand).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0";

        // Remove commit hash if present (e.g. 1.0.0+commit)
        if (version.Contains('+'))
        {
            version = version.Split('+')[0];
        }

        return $"v{version}";
    }

    private static string DetectPlatform()
    {
        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx"
            : "linux";

        var arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.OSArchitecture}")
        };

        return $"{os}-{arch}";
    }

    private static async Task<string> GetLatestVersionAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("slack-upgrader/1.0");

        var response = await client.GetFromJsonAsync(
            $"https://api.github.com/repos/{Repo}/releases/latest",
            AppJsonContext.Default.GitHubRelease);

        return response?.TagName ?? throw new Exception("Failed to get latest version");
    }

    private static bool NeedsUpgrade(string currentVersion, string latestVersion)
    {
        // Normalize versions (remove 'v' prefix)
        var current = currentVersion.TrimStart('v');
        var latest = latestVersion.TrimStart('v');

        if (Version.TryParse(current, out var currentVer) && 
            Version.TryParse(latest, out var latestVer))
        {
            return currentVer < latestVer;
        }

        // Fallback to string comparison
        return current != latest;
    }

    private static async Task DownloadAndInstallAsync(string platform, string version)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var extension = isWindows ? "zip" : "tar.gz";
        var archiveName = $"slack-{platform}.{extension}";
        var downloadUrl = $"https://github.com/{Repo}/releases/download/{version}/{archiveName}";

        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("slack-upgrader/1.0");

        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var downloadTask = ctx.AddTask(Strings.UpgradeDownloading);
                var extractTask = ctx.AddTask(Strings.UpgradeExtracting);
                var installTask = ctx.AddTask(Strings.UpgradeInstalling);

                extractTask.StopTask();
                installTask.StopTask();

                // Download
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                try
                {
                    var archivePath = Path.Combine(tempDir, archiveName);

                    using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        var totalBytes = response.Content.Headers.ContentLength ?? -1;

                        await using var fileStream = File.Create(archivePath);
                        await using var downloadStream = await response.Content.ReadAsStreamAsync();

                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;

                        while ((bytesRead = await downloadStream.ReadAsync(buffer)) > 0)
                        {
                            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                            totalRead += bytesRead;

                            if (totalBytes > 0)
                            {
                                downloadTask.Value = (double)totalRead / totalBytes * 100;
                            }
                        }
                    }

                    downloadTask.Value = 100;
                    downloadTask.StopTask();

                    // Extract
                    extractTask.StartTask();
                    var extractDir = Path.Combine(tempDir, "extracted");
                    Directory.CreateDirectory(extractDir);

                    if (isWindows)
                    {
                        ZipFile.ExtractToDirectory(archivePath, extractDir);
                    }
                    else
                    {
                        // Use tar command for .tar.gz on Unix
                        var tarProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "tar",
                            Arguments = $"-xzf \"{archivePath}\" -C \"{extractDir}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        });
                        await tarProcess!.WaitForExitAsync();
                    }

                    extractTask.Value = 100;
                    extractTask.StopTask();

                    // Install
                    installTask.StartTask();

                    var binaryFileName = isWindows ? $"{BinaryName}.exe" : BinaryName;
                    var sourceBinary = Path.Combine(extractDir, binaryFileName);

                    if (!File.Exists(sourceBinary))
                    {
                        throw new FileNotFoundException($"Binary not found: {sourceBinary}");
                    }

                    // Get current executable path
                    var currentExePath = Environment.ProcessPath
                        ?? throw new Exception("Cannot determine current executable path");

                    // On Windows, rename current executable and copy new one
                    // On Unix, we need sudo or write permission
                    if (isWindows)
                    {
                        // Windows: Launch a separate process to replace the file
                        var pid = Environment.ProcessId;
                        
                        // PowerShell script to wait for exit, replace file, and cleanup
                        var script = $@"
$ErrorActionPreference = 'Stop'
Write-Host 'Waiting for slack to exit...'
try {{ Wait-Process -Id {pid} -ErrorAction SilentlyContinue }} catch {{}}
Start-Sleep -Seconds 1

Write-Host 'Updating files...'
try {{
    Copy-Item -Path '{sourceBinary}' -Destination '{currentExePath}' -Force
    Write-Host 'Update completed successfully!' -ForegroundColor Green
}} catch {{
    Write-Host 'Update failed: $_' -ForegroundColor Red
    Read-Host 'Press Enter to exit...'
    exit 1
}}

# Cleanup temp files
try {{ Remove-Item -Path '{tempDir}' -Recurse -Force -ErrorAction SilentlyContinue }} catch {{}}

Start-Sleep -Seconds 2
";
                        // Use Base64 to avoid escaping issues
                        var encodedScript = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(script));
                        
                        var psi = new ProcessStartInfo
                        {
                            FileName = "powershell",
                            Arguments = $"-NoProfile -EncodedCommand {encodedScript}",
                            UseShellExecute = true, // Open in new window
                            CreateNoWindow = false,
                            WindowStyle = ProcessWindowStyle.Normal
                        };

                        AnsiConsole.MarkupLine("[yellow]Launching updater in a new window...[/]");
                        Process.Start(psi);
                        
                        // Exit immediately so the updater can proceed
                        // Note: C# finally blocks won't run, so tempDir is preserved for the script to use/delete
                        Environment.Exit(0);
                    }
                    else
                    {
                        // Make executable
                        System.Diagnostics.Process.Start("chmod", $"+x \"{sourceBinary}\"")?.WaitForExit();

                        // Try direct copy first
                        try
                        {
                            File.Copy(sourceBinary, currentExePath, true);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Need sudo
                            var sudoProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "sudo",
                                Arguments = $"cp \"{sourceBinary}\" \"{currentExePath}\"",
                                UseShellExecute = true
                            });
                            await sudoProcess!.WaitForExitAsync();

                            if (sudoProcess.ExitCode != 0)
                            {
                                throw new Exception("Failed to install with sudo");
                            }
                        }
                    }

                    installTask.Value = 100;
                    installTask.StopTask();
                }
                finally
                {
                    // Cleanup
                    try { Directory.Delete(tempDir, true); } catch { /* ignore */ }
                }
            });
    }

}

internal record GitHubRelease(
    [property: JsonPropertyName("tag_name")] string TagName
);
