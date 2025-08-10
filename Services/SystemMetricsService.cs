using System.Diagnostics;
using System.Text.RegularExpressions;
using ApiGateway.Models;

namespace ApiGateway.Services;

public sealed class SystemMetricsService
{
    // Return English field names; keep only human message in Vietnamese
    public async Task<SystemSummaryDto> GetSystemSummaryAsync(CancellationToken cancellationToken = default)
    {
        var os = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
        var cpuInfoTask = GetCpuInfoAsync(cancellationToken);
        var cpuUsageTask = GetCpuUsagePercentAsync(cancellationToken);
        await Task.WhenAll(cpuInfoTask, cpuUsageTask);
        var cpuInfo = await cpuInfoTask;
        var cpuUsage = await cpuUsageTask;
        var memory = GetMemoryInfo();
        var storage = GetStorageInfo();

        return new SystemSummaryDto
        {
            Name = "api-gateway",
            Time = DateTimeOffset.UtcNow,
            Os = os,
            Cpu = new CpuInfoDto
            {
                Model = cpuInfo.model,
                PhysicalCores = cpuInfo.physicalCores,
                LogicalProcessors = cpuInfo.logicalProcessors,
                AverageGHz = Math.Round(cpuInfo.avgMHz / 1000.0, 2),
                UsagePercent = Math.Round(cpuUsage, 2)
            },
            Memory = new MemoryInfoDto
            {
                Total = FormatBytesHuman(memory.total),
                Free = FormatBytesHuman(memory.free),
                UsedPercent = Math.Round(memory.usedPercent, 2)
            },
            Storage = new StorageInfoDto
            {
                Total = FormatBytesHuman(storage.total),
                Free = FormatBytesHuman(storage.free),
                Used = FormatBytesHuman(storage.used),
                UsedPercent = Math.Round(storage.usedPercent, 2)
            }
        };
    }

    public static string FormatBytesHuman(ulong bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private static async Task<double> GetCpuUsagePercentAsync(CancellationToken ct)
    {
        if (!OperatingSystem.IsLinux())
        {
            var p = Process.GetCurrentProcess();
            var start = p.TotalProcessorTime;
            var sw = Stopwatch.StartNew();
            await Task.Delay(500, ct);
            sw.Stop();
            var end = p.TotalProcessorTime;
            var cpuUsedMs = (end - start).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * sw.Elapsed.TotalMilliseconds) * 100.0;
            return Math.Clamp(cpuUsageTotal, 0, 100);
        }

        static (ulong idle, ulong total) ReadStat()
        {
            var text = File.ReadAllText("/proc/stat");
            var first = text.Split('\n').FirstOrDefault(l => l.StartsWith("cpu "));
            if (first == null) return (0, 0);
            var parts = first.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(ulong.Parse).ToArray();
            var idle = parts[3];
            var total = parts.Aggregate<ulong, ulong>(0, (acc, v) => acc + v);
            return (idle, total);
        }

        var (idle1, total1) = ReadStat();
        await Task.Delay(500, ct);
        var (idle2, total2) = ReadStat();
        var idle = idle2 - idle1;
        var total = total2 - total1;
        if (total == 0) return 0;
        var usage = (1.0 - (double)idle / total) * 100.0;
        return Math.Clamp(usage, 0, 100);
    }

    private static async Task<(string model, int physicalCores, int logicalProcessors, double avgMHz)> GetCpuInfoAsync(CancellationToken ct)
    {
        if (OperatingSystem.IsLinux())
        {
            var cpuinfo = await File.ReadAllTextAsync("/proc/cpuinfo", ct);
            string model = Regex.Matches(cpuinfo, @"^model name\s*:\s*(.+)$", RegexOptions.Multiline)
                                    .Cast<Match>()
                                    .Select(m => m.Groups[1].Value.Trim())
                    .FirstOrDefault() ?? "Unknown CPU";

            int logicalProcessors = Regex.Matches(cpuinfo, @"^processor\s*:\s*(\d+)$", RegexOptions.Multiline).Count;
            if (logicalProcessors <= 0) logicalProcessors = Environment.ProcessorCount;

            var coreCountMatches = Regex.Matches(cpuinfo, @"^cpu cores\s*:\s*(\d+)$", RegexOptions.Multiline)
                                            .Cast<Match>()
                                            .Select(m => int.Parse(m.Groups[1].Value))
                                            .ToList();
            int physicalCores = coreCountMatches.Count > 0 ? coreCountMatches.Max() : Math.Max(1, logicalProcessors);

            double avgMHz = Regex.Matches(cpuinfo, @"^cpu MHz\s*:\s*([0-9.]+)$", RegexOptions.Multiline)
                                      .Cast<Match>()
                                      .Select(m => double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))
                                      .DefaultIfEmpty(0)
                                      .Average();

            // Check for container CPU limits
            var containerCpuLimit = GetContainerCpuLimit();
            if (containerCpuLimit > 0)
            {
                // Override with container limits
                logicalProcessors = (int)Math.Max(1, Math.Ceiling(containerCpuLimit));
                physicalCores = (int)Math.Max(1, Math.Ceiling(containerCpuLimit));
                
                // Add container info to model name only if limit is actually set
                if (containerCpuLimit < Environment.ProcessorCount)
                {
                    if (containerCpuLimit < 1.0)
                    {
                        model = $"{model} (Container: {containerCpuLimit:F1} CPU cores)";
                    }
                    else
                    {
                        model = $"{model} (Container: {containerCpuLimit:F0} CPU cores)";
                    }
                }
            }

            return (model, physicalCores, logicalProcessors, avgMHz);
        }
        else
        {
        return ($"CPU {Environment.ProcessorCount} logical cores", Environment.ProcessorCount, Environment.ProcessorCount, 0);
        }
    }

    private static (ulong total, ulong free, double usedPercent) GetMemoryInfo()
    {
        if (OperatingSystem.IsLinux())
        {
            // Try to read container memory limits first
            var containerTotal = GetContainerMemoryLimit();
            if (containerTotal > 0)
            {
                // Read current memory usage from container perspective
                var containerUsed = GetContainerMemoryUsage();
                var freeBytes = containerTotal > containerUsed ? containerTotal - containerUsed : 0UL;
                var usedPercent = containerTotal > 0 ? (double)containerUsed / containerTotal * 100.0 : 0.0;
                
                return (containerTotal, freeBytes, usedPercent);
            }
            else
            {
                // Fallback to reading from /proc/meminfo
                var meminfo = File.ReadAllLines("/proc/meminfo");
                ulong ReadKb(string key)
                {
                    var line = meminfo.FirstOrDefault(l => l.StartsWith(key, StringComparison.OrdinalIgnoreCase));
                    if (line == null) return 0;
                    var parts = Regex.Split(line, @"\s+");
                    if (parts.Length < 2) return 0;
                    if (ulong.TryParse(parts[1], out var kb)) return kb;
                    return 0;
                }

                var totalKb = ReadKb("MemTotal:");
                var freeKb = ReadKb("MemAvailable:");
                if (freeKb == 0) freeKb = ReadKb("MemFree:");
                var usedPercent = totalKb > 0 ? (1.0 - (double)freeKb / totalKb) * 100.0 : 0.0;
                return (totalKb * 1024, freeKb * 1024, usedPercent);
            }
        }
        else
        {
            var gcInfo = GC.GetGCMemoryInfo();
            var total = (ulong)gcInfo.TotalAvailableMemoryBytes;
            var used = (ulong)GC.GetTotalMemory(false);
            var free = total > used ? total - used : 0UL;
            var usedPct = total > 0 ? (double)used / total * 100.0 : 0.0;
        return (total, free, usedPct);
        }
    }

    private static (ulong total, ulong free, ulong used, double usedPercent) GetStorageInfo()
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && (d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable));
        ulong total = 0, free = 0;
        foreach (var d in drives)
            {
                try
                {
                    total += (ulong)d.TotalSize;
                    free += (ulong)d.TotalFreeSpace;
                }
                catch { }
            }
        
        var used = total > free ? total - free : 0UL;
        var usedPercent = total > 0 ? (double)used / total * 100.0 : 0.0;
        
        return (total, free, used, usedPercent);
        }
        catch
        {
        return (0, 0, 0, 0);
        }
    }

    private static ulong GetContainerMemoryLimit()
    {
        try
        {
            // Try to read cgroup v2 memory limit
            if (File.Exists("/sys/fs/cgroup/memory.max"))
            {
                var content = File.ReadAllText("/sys/fs/cgroup/memory.max").Trim();
                if (content != "max" && ulong.TryParse(content, out var limit))
                {
                    return limit;
                }
            }

            // Try to read cgroup v1 memory limit
            if (File.Exists("/sys/fs/cgroup/memory/memory.limit_in_bytes"))
            {
                var content = File.ReadAllText("/sys/fs/cgroup/memory/memory.limit_in_bytes").Trim();
                if (ulong.TryParse(content, out var limit))
                {
                    // Very large values indicate no limit set (usually 9223372036854775807 or similar)
                    if (limit < (1UL << 62)) // Less than ~4.6 exabytes
                    {
                        return limit;
                    }
                }
            }

            // Check for container memory stats
            if (File.Exists("/sys/fs/cgroup/memory/memory.stat"))
            {
                var content = File.ReadAllText("/sys/fs/cgroup/memory/memory.stat");
                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("hierarchical_memory_limit "))
                    {
                        var parts = line.Split(' ');
                        if (parts.Length > 1 && ulong.TryParse(parts[1], out var hierarchicalLimit))
                        {
                            if (hierarchicalLimit < (1UL << 62))
                            {
                                return hierarchicalLimit;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore errors and fall back to host info
        }

        return 0; // No container limit found
    }

    private static ulong GetContainerMemoryUsage()
    {
        try
        {
            // Try to read cgroup v2 memory usage
            if (File.Exists("/sys/fs/cgroup/memory.current"))
            {
                var content = File.ReadAllText("/sys/fs/cgroup/memory.current").Trim();
                if (ulong.TryParse(content, out var usage))
                {
                    return usage;
                }
            }

            // Try to read cgroup v1 memory usage
            if (File.Exists("/sys/fs/cgroup/memory/memory.usage_in_bytes"))
            {
                var content = File.ReadAllText("/sys/fs/cgroup/memory/memory.usage_in_bytes").Trim();
                if (ulong.TryParse(content, out var usage))
                {
                    return usage;
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return 0; // No container usage info found
    }

    private static double GetContainerCpuLimit()
    {
        try
        {
            // Try to read cgroup v2 CPU limit
            if (File.Exists("/sys/fs/cgroup/cpu.max"))
            {
                var content = File.ReadAllText("/sys/fs/cgroup/cpu.max").Trim();
                var parts = content.Split(' ');
                if (parts.Length == 2 && parts[0] != "max")
                {
                    if (double.TryParse(parts[0], out var quota) && double.TryParse(parts[1], out var period))
                    {
                        return quota / period; // Returns fractional CPU cores
                    }
                }
            }

            // Try to read cgroup v1 CPU limit
            if (File.Exists("/sys/fs/cgroup/cpu/cpu.cfs_quota_us") && File.Exists("/sys/fs/cgroup/cpu/cpu.cfs_period_us"))
            {
                var quotaContent = File.ReadAllText("/sys/fs/cgroup/cpu/cpu.cfs_quota_us").Trim();
                var periodContent = File.ReadAllText("/sys/fs/cgroup/cpu/cpu.cfs_period_us").Trim();
                
                if (quotaContent != "-1" && 
                    double.TryParse(quotaContent, out var quota) && 
                    double.TryParse(periodContent, out var period) && period > 0)
                {
                    return quota / period; // Returns fractional CPU cores
                }
            }

            // Check for CPU shares (less precise but still useful)
            if (File.Exists("/sys/fs/cgroup/cpu/cpu.shares"))
            {
                var sharesContent = File.ReadAllText("/sys/fs/cgroup/cpu/cpu.shares").Trim();
                if (double.TryParse(sharesContent, out var shares))
                {
                    // Default shares is typically 1024, so we can estimate
                    // This is less accurate but gives some indication
                    if (shares < 1024)
                    {
                        return shares / 1024.0;
                    }
                }
            }
        }
        catch
        {
            // Ignore errors and fall back to host info
        }

        return 0; // No container limit found
    }
}
