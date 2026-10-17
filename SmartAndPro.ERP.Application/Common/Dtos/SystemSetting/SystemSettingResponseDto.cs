using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;
using SmartAndPro.ERP.Application.Common.Dtos.Recipes.RecipeEntry;
using SmartAndPro.ERP.Domains.Enums;

namespace SmartAndPro.ERP.Application.Common.Dtos.SystemSetting;

public class SystemSettingResponseDto
{
    public Guid Id { get; set; }

    public string TimeZone { get; set; }
    public bool IsLinkedToInventory { get; set; } = false;
    public bool? IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public ReservationSerialNumberFormat? ReservationSerialNumberFormat { get; set; }
    public long? ReservationSerialStartNumber { get; set; }
    public string? ReservationSerialPrefix { get; set; }
    public bool ShowSignaturesInInvoicePrint { get; set; }
    public bool ShowTermsInPrint2 { get; set; }

    public bool ShowStampInReservationPrint { get; set; } = true;
    public bool ShowCompanySignatureInReservationPrint { get; set; } = true;
    public bool ShowCustomerSignatureInReservationPrint { get; set; } = true;
    public List<string> DefaultReservationUncheckedPolicyIds { get; set; } = new();
    public List<string> DefaultReservationUncheckedBankIds { get; set; } = new();
    public int SessionTimeoutMinutes { get; set; } = 20;
}
