using SmartAndPro.ERP.Application.Common.Dtos.Accounting.ClientProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.MeasurementUnits;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;

public class MainSalesClassResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public string ReferenceCode { get; set; }
    
    public Guid DetailSalesClassId { get; set; }
    public Guid ClientProfileId { get; set; }
    
    public Guid MeasurementUnitId { get; set; }
    public MeasurementUnitResponseDto MeasurementUnit { get; set; }
    public string ClassType { get; set; } 
    
    public bool IsTaxExempted { get; set; } = false;
    public bool IsReservationItem { get; set; } = false;

    
    public ClientProfileResponseDto ClientProfile { get; set; }
    
    public string? Icon { get; set; }

    public decimal ProductUnitsCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; }
}