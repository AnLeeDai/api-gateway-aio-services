namespace ApiGateway.Models;

public sealed class SystemSummaryDto
{
    public string Name { get; set; } = "api-gateway";
    public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
    public string Os { get; set; } = string.Empty;
    public CpuInfoDto Cpu { get; set; } = new();
    public MemoryInfoDto Memory { get; set; } = new();
    public StorageInfoDto Storage { get; set; } = new();
    public Dictionary<string, object>? Services { get; set; }
}

public sealed class CpuInfoDto
{
    public string Model { get; set; } = string.Empty;
    public int PhysicalCores { get; set; }
    public int LogicalProcessors { get; set; }
    public double AverageGHz { get; set; }
    public double UsagePercent { get; set; }
}

public sealed class MemoryInfoDto
{
    public string Total { get; set; } = string.Empty;
    public string Free { get; set; } = string.Empty;
    public double UsedPercent { get; set; }
}

public sealed class StorageInfoDto
{
    public string Total { get; set; } = string.Empty;
    public string Free { get; set; } = string.Empty;
    public string Used { get; set; } = string.Empty;
    public double UsedPercent { get; set; }
}
