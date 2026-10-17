using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.FingerprintDeviceSettings;

public class FingerprintDeviceSettingsPostDto
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
    
    public bool? IsActive { get; set; } = true;
}