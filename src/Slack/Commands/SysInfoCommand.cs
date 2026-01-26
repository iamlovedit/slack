using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Slack.Localization;
using Spectre.Console;

using Slack.Infrastructure;

namespace Slack.Commands;

[AutoRegisterCommand]
public class SysInfoCommand : ICommand
{
    public string Name => "sysinfo";
    public string Description => Strings.SysinfoDescription;
    public string[] Aliases => ["sys", "info"];

    // Network tracking for speed calculation
    private long _lastBytesReceived;
    private long _lastBytesSent;
    private DateTime _lastNetworkCheck = DateTime.MinValue;

    // CPU tracking
    private TimeSpan _lastTotalCpuTime;
    private DateTime _lastCpuCheck = DateTime.MinValue;

    // Disk I/O tracking
    private long _lastDiskRead;
    private long _lastDiskWrite;
    private DateTime _lastDiskCheck = DateTime.MinValue;

    public int Execute(string[] args)
    {
        var table = CreateTable();
        
        AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Start(ctx =>
            {
                while (!Console.KeyAvailable)
                {
                    UpdateTable(table);
                    ctx.Refresh();
                    Thread.Sleep(1000);
                }
                // Consume the key
                Console.ReadKey(true);
            });

        return 0;
    }

    private Table CreateTable()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold cyan]{Strings.SysinfoTitle}[/]")
            .AddColumn(new TableColumn("[bold]Category[/]").Width(12))
            .AddColumn(new TableColumn("[bold]Device[/]").Width(35))
            .AddColumn(new TableColumn("[bold]Status[/]").Width(30));

        return table;
    }

    private void UpdateTable(Table table)
    {
        table.Rows.Clear();

        // CPU
        var cpuName = GetCpuName();
        var cpuUsage = GetCpuUsage();
        table.AddRow(
            $"[yellow]CPU[/]",
            $"[dim]{TruncateText(cpuName, 33)}[/]",
            CreateProgressBar(cpuUsage, "green")
        );

        // GPU
        var (gpuName, gpuUsage, gpuMemUsed, gpuMemTotal) = GetGpuInfo();
        if (gpuUsage >= 0)
        {
            var gpuStatus = CreateProgressBar(gpuUsage, "magenta");
            if (gpuMemTotal > 0)
            {
                gpuStatus += $" [dim]({FormatBytes(gpuMemUsed)})[/]";
            }
            table.AddRow(
                $"[yellow]GPU[/]",
                $"[dim]{TruncateText(gpuName, 33)}[/]",
                gpuStatus
            );
        }
        else
        {
            table.AddRow(
                $"[yellow]GPU[/]",
                $"[dim]{Strings.SysinfoGpuUnavailable}[/]",
                "[dim]-[/]"
            );
        }

        // Memory
        var (memUsed, memTotal) = GetMemoryInfo();
        var memPercent = memTotal > 0 ? (double)memUsed / memTotal * 100 : 0;
        table.AddRow(
            $"[yellow]{Strings.SysinfoMemory}[/]",
            $"[dim]{FormatBytes(memUsed)} / {FormatBytes(memTotal)}[/]",
            CreateProgressBar(memPercent, "blue")
        );

        // Disk I/O - per disk
        var disks = GetPerDiskIoSpeed();
        foreach (var disk in disks)
        {
            table.AddRow(
                $"[yellow]{Strings.SysinfoDisk}[/]",
                $"[dim]{disk.Name}[/]",
                $"[green]↓{FormatSpeed(disk.ReadSpeed)}[/] [red]↑{FormatSpeed(disk.WriteSpeed)}[/]"
            );
        }
        if (disks.Count == 0)
        {
            table.AddRow(
                $"[yellow]{Strings.SysinfoDisk}[/]",
                "[dim]-[/]",
                "[dim]-[/]"
            );
        }

        // Network - per interface
        var interfaces = GetPerInterfaceNetworkSpeed();
        foreach (var iface in interfaces)
        {
            table.AddRow(
                $"[yellow]{Strings.SysinfoNetwork}[/]",
                $"[dim]{TruncateText(iface.Name, 33)}[/]",
                $"[green]↓{FormatSpeed(iface.DownloadSpeed)}[/] [red]↑{FormatSpeed(iface.UploadSpeed)}[/]"
            );
        }
        if (interfaces.Count == 0)
        {
            table.AddRow(
                $"[yellow]{Strings.SysinfoNetwork}[/]",
                "[dim]-[/]",
                "[dim]-[/]"
            );
        }

        // Footer
        table.AddEmptyRow();
        table.AddRow(
            "",
            $"[dim italic]{Strings.SysinfoPressAnyKey}[/]",
            ""
        );
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Length <= maxLength ? text : text[..(maxLength - 2)] + "..";
    }

    private static string CreateProgressBar(double percent, string color)
    {
        const int width = 20;
        var filled = (int)(percent / 100 * width);
        var empty = width - filled;
        var bar = new string('█', filled) + new string('░', empty);
        return $"[{color}]{bar}[/]  {percent:F1}%";
    }

    #region CPU

    private string? _cachedCpuName;

    private string GetCpuName()
    {
        if (_cachedCpuName != null) return _cachedCpuName;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _cachedCpuName = GetWindowsCpuName();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _cachedCpuName = GetLinuxCpuName();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _cachedCpuName = GetMacOsCpuName();
        }
        else
        {
            _cachedCpuName = "Unknown CPU";
        }

        return _cachedCpuName;
    }

    private string GetWindowsCpuName()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-NoProfile -Command \"(Get-CimInstance Win32_Processor).Name\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return "Unknown CPU";

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(2000);
            return string.IsNullOrEmpty(output) ? "Unknown CPU" : output;
        }
        catch
        {
            return "Unknown CPU";
        }
    }

    private string GetLinuxCpuName()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/cpuinfo");
            foreach (var line in lines)
            {
                if (line.StartsWith("model name"))
                {
                    var parts = line.Split(':', 2);
                    if (parts.Length >= 2)
                    {
                        return parts[1].Trim();
                    }
                }
            }
        }
        catch
        {
            // Ignore
        }
        return "Unknown CPU";
    }

    private string GetMacOsCpuName()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sysctl",
                Arguments = "-n machdep.cpu.brand_string",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return "Unknown CPU";

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return string.IsNullOrEmpty(output) ? "Unknown CPU" : output;
        }
        catch
        {
            return "Unknown CPU";
        }
    }

    private double GetCpuUsage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsCpuUsage();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxCpuUsage();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetMacOsCpuUsage();
        }
        return 0;
    }

    private double GetWindowsCpuUsage()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var currentTime = DateTime.UtcNow;
            var currentCpuTime = process.TotalProcessorTime;

            if (_lastCpuCheck == DateTime.MinValue)
            {
                _lastCpuCheck = currentTime;
                _lastTotalCpuTime = currentCpuTime;
                return 0;
            }

            var elapsed = (currentTime - _lastCpuCheck).TotalMilliseconds;
            var cpuUsed = (currentCpuTime - _lastTotalCpuTime).TotalMilliseconds;
            
            _lastCpuCheck = currentTime;
            _lastTotalCpuTime = currentCpuTime;

            var cpuPercent = cpuUsed / (elapsed * Environment.ProcessorCount) * 100;
            
            // Get system-wide CPU using a simpler approach
            // For now return process CPU - to get system CPU we'd need P/Invoke or perf counters
            return Math.Min(100, Math.Max(0, cpuPercent * Environment.ProcessorCount));
        }
        catch
        {
            return 0;
        }
    }

    private double GetLinuxCpuUsage()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/stat");
            var cpuLine = lines.FirstOrDefault(l => l.StartsWith("cpu "));
            if (cpuLine == null) return 0;

            var parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) return 0;

            var user = long.Parse(parts[1]);
            var nice = long.Parse(parts[2]);
            var system = long.Parse(parts[3]);
            var idle = long.Parse(parts[4]);
            var total = user + nice + system + idle;

            // Simple approximation
            var busy = user + nice + system;
            return total > 0 ? (double)busy / total * 100 : 0;
        }
        catch
        {
            return 0;
        }
    }

    private double GetMacOsCpuUsage()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "top",
                Arguments = "-l 1 -n 0",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return 0;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse "CPU usage: X% user, Y% sys, Z% idle"
            var match = System.Text.RegularExpressions.Regex.Match(
                output, @"CPU usage:\s*([\d.]+)%\s*user,\s*([\d.]+)%\s*sys");
            
            if (match.Success)
            {
                var user = double.Parse(match.Groups[1].Value);
                var sys = double.Parse(match.Groups[2].Value);
                return user + sys;
            }
        }
        catch
        {
            // Ignore
        }
        return 0;
    }

    #endregion

    #region GPU

    private string? _cachedGpuName;

    private (string name, double usage, long memUsed, long memTotal) GetGpuInfo()
    {
        // Try nvidia-smi first (works on Windows and Linux)
        var nvResult = TryGetNvidiaGpuInfo();
        if (nvResult.usage >= 0) return nvResult;

        // Try Windows Performance Counters (for AMD/Intel on Windows)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var winResult = TryGetWindowsGpuInfo();
            if (winResult.usage >= 0) return winResult;
        }

        // Try AMD ROCm-SMI on Linux
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var amdResult = TryGetAmdGpuInfo();
            if (amdResult.usage >= 0) return amdResult;
        }

        return ("N/A", -1, 0, 0);
    }

    private (string name, double usage, long memUsed, long memTotal) TryGetNvidiaGpuInfo()
    {
        try
        {
            // Get GPU name (cached)
            if (_cachedGpuName == null)
            {
                var namePsi = new ProcessStartInfo
                {
                    FileName = "nvidia-smi",
                    Arguments = "--query-gpu=name --format=csv,noheader",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var nameProcess = Process.Start(namePsi);
                if (nameProcess != null)
                {
                    _cachedGpuName = nameProcess.StandardOutput.ReadToEnd().Trim();
                    nameProcess.WaitForExit(2000);
                }
            }

            var psi = new ProcessStartInfo
            {
                FileName = "nvidia-smi",
                Arguments = "--query-gpu=utilization.gpu,memory.used,memory.total --format=csv,noheader,nounits",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return ("N/A", -1, 0, 0);

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode != 0 || string.IsNullOrEmpty(output))
                return ("N/A", -1, 0, 0);

            var parts = output.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length >= 3)
            {
                var usage = double.Parse(parts[0]);
                var memUsed = long.Parse(parts[1]) * 1024 * 1024; // MB to bytes
                var memTotal = long.Parse(parts[2]) * 1024 * 1024;
                return (_cachedGpuName ?? "NVIDIA GPU", usage, memUsed, memTotal);
            }
        }
        catch
        {
            // nvidia-smi not available
        }
        return ("N/A", -1, 0, 0);
    }

    private (string name, double usage, long memUsed, long memTotal) TryGetWindowsGpuInfo()
    {
        // Simplified - would need P/Invoke for full implementation
        // Performance counters require elevation or special setup
        return ("N/A", -1, 0, 0);
    }

    private (string name, double usage, long memUsed, long memTotal) TryGetAmdGpuInfo()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "rocm-smi",
                Arguments = "--showproductname --showuse --showmemuse",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return ("N/A", -1, 0, 0);

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0) return ("N/A", -1, 0, 0);

            // Parse rocm-smi output
            var nameMatch = System.Text.RegularExpressions.Regex.Match(output, @"Card series:\s*(.+)");
            var usageMatch = System.Text.RegularExpressions.Regex.Match(output, @"GPU use \(%\):\s*(\d+)");
            
            if (usageMatch.Success)
            {
                var gpuName = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : "AMD GPU";
                return (gpuName, double.Parse(usageMatch.Groups[1].Value), 0, 0);
            }
        }
        catch
        {
            // rocm-smi not available
        }
        return ("N/A", -1, 0, 0);
    }

    #endregion

    #region Memory

    private (long used, long total) GetMemoryInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsMemoryInfo();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxMemoryInfo();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetMacOsMemoryInfo();
        }
        return (0, 0);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    private (long used, long total) GetWindowsMemoryInfo()
    {
        try
        {
            var memStatus = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
            if (GlobalMemoryStatusEx(ref memStatus))
            {
                var total = (long)memStatus.ullTotalPhys;
                var available = (long)memStatus.ullAvailPhys;
                return (total - available, total);
            }
        }
        catch
        {
            // Ignore
        }
        return (0, 0);
    }

    private (long used, long total) GetLinuxMemoryInfo()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            long total = 0, available = 0;

            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal:"))
                {
                    total = ParseMemInfoValue(line) * 1024; // KB to bytes
                }
                else if (line.StartsWith("MemAvailable:"))
                {
                    available = ParseMemInfoValue(line) * 1024;
                }
            }

            return (total - available, total);
        }
        catch
        {
            return (0, 0);
        }
    }

    private static long ParseMemInfoValue(string line)
    {
        var parts = line.Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return 0;
        var valuePart = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        return long.TryParse(valuePart, out var value) ? value : 0;
    }

    private (long used, long total) GetMacOsMemoryInfo()
    {
        try
        {
            // Get total memory
            var psi = new ProcessStartInfo
            {
                FileName = "sysctl",
                Arguments = "-n hw.memsize",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return (0, 0);

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (long.TryParse(output, out var total))
            {
                // Get used memory from vm_stat (simplified)
                var gcInfo = GC.GetGCMemoryInfo();
                var used = total - gcInfo.TotalAvailableMemoryBytes;
                return (used, total);
            }
        }
        catch
        {
            // Ignore
        }
        return (0, 0);
    }

    #endregion

    #region Disk

    private record DiskInfo(string Name, long ReadSpeed, long WriteSpeed);
    
    private readonly Dictionary<string, (long read, long write, DateTime time)> _diskStats = new();

    private List<DiskInfo> GetPerDiskIoSpeed()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsPerDiskIoSpeed();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxPerDiskIoSpeed();
        }
        return [];
    }

    private List<DiskInfo> GetWindowsPerDiskIoSpeed()
    {
        var result = new List<DiskInfo>();
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .ToList();

            foreach (var drive in drives)
            {
                var name = drive.Name.TrimEnd('\\', '/');
                // For Windows, we use simplified approach - show drive letters
                // Full per-disk I/O requires performance counters
                result.Add(new DiskInfo(name, 0, 0));
            }

            // Get aggregate I/O and distribute to first disk for now
            var (totalRead, totalWrite) = GetWindowsDiskIoSpeed();
            if (result.Count > 0)
            {
                result[0] = new DiskInfo(result[0].Name, totalRead, totalWrite);
            }
        }
        catch
        {
            // Ignore
        }
        return result;
    }

    private List<DiskInfo> GetLinuxPerDiskIoSpeed()
    {
        var result = new List<DiskInfo>();
        try
        {
            var lines = File.ReadAllLines("/proc/diskstats");
            var now = DateTime.UtcNow;

            foreach (var line in lines)
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 14)
                {
                    var deviceName = parts[2];
                    // Only count main disks
                    bool isMainDisk = 
                        (deviceName.StartsWith("sd") && deviceName.Length == 3) ||
                        (deviceName.StartsWith("nvme") && deviceName.Contains("n") && !deviceName.Contains("p")) ||
                        (deviceName.StartsWith("vd") && deviceName.Length == 3);
                    
                    if (!isMainDisk) continue;

                    if (long.TryParse(parts[5], out var sectorsRead) && 
                        long.TryParse(parts[9], out var sectorsWrite))
                    {
                        var bytesRead = sectorsRead * 512;
                        var bytesWrite = sectorsWrite * 512;

                        long readSpeed = 0, writeSpeed = 0;
                        if (_diskStats.TryGetValue(deviceName, out var lastStats))
                        {
                            var elapsed = (now - lastStats.time).TotalSeconds;
                            if (elapsed > 0)
                            {
                                readSpeed = (long)((bytesRead - lastStats.read) / elapsed);
                                writeSpeed = (long)((bytesWrite - lastStats.write) / elapsed);
                            }
                        }

                        _diskStats[deviceName] = (bytesRead, bytesWrite, now);
                        result.Add(new DiskInfo(deviceName.ToUpper(), Math.Max(0, readSpeed), Math.Max(0, writeSpeed)));
                    }
                }
            }
        }
        catch
        {
            // Ignore
        }
        return result;
    }

    private (long readSpeed, long writeSpeed) GetDiskIoSpeed()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsDiskIoSpeed();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxDiskIoSpeed();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetMacOsDiskIoSpeed();
        }
        return (0, 0);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetProcessIoCounters(IntPtr hProcess, out IO_COUNTERS lpIoCounters);

    private (long readSpeed, long writeSpeed) GetWindowsDiskIoSpeed()
    {
        try
        {
            // Get system-wide disk I/O using performance data
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-NoProfile -Command \"$d = Get-Counter '\\PhysicalDisk(_Total)\\Disk Read Bytes/sec','\\PhysicalDisk(_Total)\\Disk Write Bytes/sec' -ErrorAction SilentlyContinue; if($d){$d.CounterSamples.CookedValue -join ','}else{'0,0'}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return (0, 0);

            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(2000);

            var parts = output.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length >= 2)
            {
                if (double.TryParse(parts[0], out var read) && double.TryParse(parts[1], out var write))
                {
                    return ((long)read, (long)write);
                }
            }
        }
        catch
        {
            // Fallback to process I/O
        }
        return (0, 0);
    }

    private (long readSpeed, long writeSpeed) GetLinuxDiskIoSpeed()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/diskstats");
            long totalRead = 0, totalWrite = 0;

            foreach (var line in lines)
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 14)
                {
                    var deviceName = parts[2];
                    // Only count main disks (sda, nvme0n1, etc.), not partitions
                    if (deviceName.StartsWith("sd") && deviceName.Length == 3 ||
                        deviceName.StartsWith("nvme") && deviceName.Contains("n") && !deviceName.Contains("p") ||
                        deviceName.StartsWith("vd") && deviceName.Length == 3)
                    {
                        // sectors read (index 5) and sectors written (index 9)
                        if (long.TryParse(parts[5], out var sectorsRead))
                            totalRead += sectorsRead * 512; // 512 bytes per sector
                        if (long.TryParse(parts[9], out var sectorsWrite))
                            totalWrite += sectorsWrite * 512;
                    }
                }
            }

            var now = DateTime.UtcNow;
            if (_lastDiskCheck == DateTime.MinValue)
            {
                _lastDiskRead = totalRead;
                _lastDiskWrite = totalWrite;
                _lastDiskCheck = now;
                return (0, 0);
            }

            var elapsed = (now - _lastDiskCheck).TotalSeconds;
            if (elapsed <= 0) return (0, 0);

            var readSpeed = (long)((totalRead - _lastDiskRead) / elapsed);
            var writeSpeed = (long)((totalWrite - _lastDiskWrite) / elapsed);

            _lastDiskRead = totalRead;
            _lastDiskWrite = totalWrite;
            _lastDiskCheck = now;

            return (Math.Max(0, readSpeed), Math.Max(0, writeSpeed));
        }
        catch
        {
            return (0, 0);
        }
    }

    private (long readSpeed, long writeSpeed) GetMacOsDiskIoSpeed()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "iostat",
                Arguments = "-d -c 2 -w 1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return (0, 0);

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(3000);

            // Parse iostat output (simplified - get last line of data)
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                var lastLine = lines[^1];
                var parts = lastLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    // iostat shows KB/s by default
                    if (double.TryParse(parts[1], out var read) && double.TryParse(parts[2], out var write))
                    {
                        return ((long)(read * 1024), (long)(write * 1024));
                    }
                }
            }
        }
        catch
        {
            // Ignore
        }
        return (0, 0);
    }

    #endregion

    #region Network

    private record InterfaceInfo(string Name, long DownloadSpeed, long UploadSpeed);
    
    private readonly Dictionary<string, (long received, long sent, DateTime time)> _interfaceStats = new();

    private List<InterfaceInfo> GetPerInterfaceNetworkSpeed()
    {
        var result = new List<InterfaceInfo>();
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                           n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                           n.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .Take(5); // Limit to 5 interfaces

            var now = DateTime.UtcNow;

            foreach (var ni in interfaces)
            {
                var stats = ni.GetIPStatistics();
                var bytesReceived = stats.BytesReceived;
                var bytesSent = stats.BytesSent;
                var id = ni.Id;

                long downloadSpeed = 0, uploadSpeed = 0;
                if (_interfaceStats.TryGetValue(id, out var lastStats))
                {
                    var elapsed = (now - lastStats.time).TotalSeconds;
                    if (elapsed > 0)
                    {
                        downloadSpeed = (long)((bytesReceived - lastStats.received) / elapsed);
                        uploadSpeed = (long)((bytesSent - lastStats.sent) / elapsed);
                    }
                }

                _interfaceStats[id] = (bytesReceived, bytesSent, now);

                // Get friendly name, truncate if too long
                var name = ni.Name;
                result.Add(new InterfaceInfo(name, Math.Max(0, downloadSpeed), Math.Max(0, uploadSpeed)));
            }
        }
        catch
        {
            // Ignore
        }
        return result;
    }

    private (long download, long upload) GetNetworkSpeed()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                           n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            long totalReceived = 0, totalSent = 0;

            foreach (var ni in interfaces)
            {
                var stats = ni.GetIPStatistics();
                totalReceived += stats.BytesReceived;
                totalSent += stats.BytesSent;
            }

            var now = DateTime.UtcNow;
            
            if (_lastNetworkCheck == DateTime.MinValue)
            {
                _lastBytesReceived = totalReceived;
                _lastBytesSent = totalSent;
                _lastNetworkCheck = now;
                return (0, 0);
            }

            var elapsed = (now - _lastNetworkCheck).TotalSeconds;
            if (elapsed <= 0) return (0, 0);

            var downloadSpeed = (long)((totalReceived - _lastBytesReceived) / elapsed);
            var uploadSpeed = (long)((totalSent - _lastBytesSent) / elapsed);

            _lastBytesReceived = totalReceived;
            _lastBytesSent = totalSent;
            _lastNetworkCheck = now;

            return (downloadSpeed, uploadSpeed);
        }
        catch
        {
            return (0, 0);
        }
    }

    #endregion

    #region Formatting

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        int i = 0;
        double size = bytes;

        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }

        return $"{size:F1} {suffixes[i]}";
    }

    private static string FormatSpeed(long bytesPerSecond)
    {
        string[] suffixes = ["B/s", "KB/s", "MB/s", "GB/s"];
        int i = 0;
        double speed = bytesPerSecond;

        while (speed >= 1024 && i < suffixes.Length - 1)
        {
            speed /= 1024;
            i++;
        }

        return $"{speed:F1} {suffixes[i]}";
    }

    #endregion
}
