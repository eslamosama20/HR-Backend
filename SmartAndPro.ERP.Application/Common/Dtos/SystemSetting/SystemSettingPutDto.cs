using SmartAndPro.ERP.Application.Common.Dtos.Recipes.RecipeEntry;
using SmartAndPro.ERP.Domains.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.SystemSetting;

public class SystemSettingPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }
    [JsonInclude]
    [Required(ErrorMessage = "TimeZone Field is required")]
    public string TimeZone { get; set; }
    public bool IsLinkedToInventory { get; set; } = false;
    public ReservationSerialNumberFormat? ReservationSerialNumberFormat { get; set; }   
    public long? ReservationSerialStartNumber { get; set; } = 1;
    public string? ReservationSerialPrefix { get; set; }
    public bool ShowSignaturesInInvoicePrint { get; set; } = true;
    public bool ShowTermsInPrint2 { get; set; }

    public bool ShowStampInReservationPrint { get; set; } = true;
    public bool ShowCompanySignatureInReservationPrint { get; set; } = true;
    public bool ShowCustomerSignatureInReservationPrint { get; set; } = true;
    public List<string> DefaultReservationUncheckedPolicyIds { get; set; } = new();
    public List<string> DefaultReservationUncheckedBankIds { get; set; } = new();
    [Range(1, 30, ErrorMessage = "Session timeout must be between 1 and 30 minutes")]
    public int SessionTimeoutMinutes { get; set; } = 20;
}
