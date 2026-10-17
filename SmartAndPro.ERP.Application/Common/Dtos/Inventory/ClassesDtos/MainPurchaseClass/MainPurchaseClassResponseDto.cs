using SmartAndPro.ERP.Application.Common.Dtos.Accounting.ClientProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.MeasurementUnits;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;

public class MainPurchaseClassResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public string ReferenceCode { get; set; }
    
    public Guid DetailPurchaseClassId { get; set; }
    public Guid ClientProfileId { get; set; }

    public Guid MeasurementUnitId { get; set; }

    public MeasurementUnitResponseDto MeasurementUnit { get; set; }
    public ClientProfileResponseDto ClientProfile { get; set; }

    public decimal ProductUnitsCount { get; set; } = 0;
    
    public string? Icon { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    
    // prices
    public decimal? LastPrice { get; set; } = 0;
    public decimal? LastMonthAveragePrice { get; set; } = 0;

    // Pharmacy fields
    public string? ItemType { get; set; }
    public bool NeedsBatch { get; set; }
    public bool NeedsExpiry { get; set; }
    public bool IsPharmacyItem { get; set; }
    public string? SfdaCode { get; set; }
    public bool IsControlled { get; set; }
}