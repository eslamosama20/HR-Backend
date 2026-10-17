using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.FingerprintDeviceSettings;

public class FingerprintDeviceSettingsResponseDto
{
    public Guid Id { get; set; }

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
    public string? Factory1LastSyncStatus { get; set; }
    public string? Factory1LastSyncMessage { get; set; }
    public int Factory1LastSyncFetchedCount { get; set; }
    public int Factory1LastSyncInsertedCount { get; set; }
    public int Factory1LastSyncSkippedCount { get; set; }

    // Factory 2 Sync Status
    public DateTime? Factory2LastSyncTime { get; set; }
    public string? Factory2LastSyncStatus { get; set; }
    public string? Factory2LastSyncMessage { get; set; }
    public int Factory2LastSyncFetchedCount { get; set; }
    public int Factory2LastSyncInsertedCount { get; set; }
    public int Factory2LastSyncSkippedCount { get; set; }

    // Factory 3 Sync Status
    public DateTime? Factory3LastSyncTime { get; set; }
    public string? Factory3LastSyncStatus { get; set; }
    public string? Factory3LastSyncMessage { get; set; }
    public int Factory3LastSyncFetchedCount { get; set; }
    public int Factory3LastSyncInsertedCount { get; set; }
    public int Factory3LastSyncSkippedCount { get; set; }
        
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}