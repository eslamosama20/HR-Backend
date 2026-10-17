using SmartAndPro.ERP.Domains.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartAndPro.ERP.Domains.Entities;

public class FingerprintDeviceSettings : Entity, IEntityTypeConfiguration<FingerprintDeviceSettings>
{
    public string? Factory1DeviceIP { get; set; }
    public string? Factory1DevicePort { get; set; }
    public bool Factory1DeviceEnabled { get; set; } = false;
    
    public string? Factory2DeviceIP { get; set; }
    public string? Factory2DevicePort { get; set; }
    public bool Factory2DeviceEnabled { get; set; } = false;

    public string? Factory3DeviceIP { get; set; }
    public string? Factory3DevicePort { get; set; }
    public bool Factory3DeviceEnabled { get; set; } = false;

    // Factory 1 Sync Status
    public DateTime? Factory1LastSyncTime { get; set; }
    public string? Factory1LastSyncStatus { get; set; } = "Never"; // Never, Running, Success, Failed
    public string? Factory1LastSyncMessage { get; set; }
    public int Factory1LastSyncFetchedCount { get; set; } = 0;
    public int Factory1LastSyncInsertedCount { get; set; } = 0;
    public int Factory1LastSyncSkippedCount { get; set; } = 0;

    // Factory 2 Sync Status
    public DateTime? Factory2LastSyncTime { get; set; }
    public string? Factory2LastSyncStatus { get; set; } = "Never"; // Never, Running, Success, Failed
    public string? Factory2LastSyncMessage { get; set; }
    public int Factory2LastSyncFetchedCount { get; set; } = 0;
    public int Factory2LastSyncInsertedCount { get; set; } = 0;
    public int Factory2LastSyncSkippedCount { get; set; } = 0;

    // Factory 3 Sync Status
    public DateTime? Factory3LastSyncTime { get; set; }
    public string? Factory3LastSyncStatus { get; set; } = "Never"; // Never, Running, Success, Failed
    public string? Factory3LastSyncMessage { get; set; }
    public int Factory3LastSyncFetchedCount { get; set; } = 0;
    public int Factory3LastSyncInsertedCount { get; set; } = 0;
    public int Factory3LastSyncSkippedCount { get; set; } = 0;

    public void Configure(EntityTypeBuilder<FingerprintDeviceSettings> builder)
    {
       
    }
}