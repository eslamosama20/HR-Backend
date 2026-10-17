using SmartAndPro.ERP.Application.Common.Dtos.Accounting.ClientProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.DetailSalesClass;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.HeaderSalesClass;

public class HeaderSalesClassResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public string ReferenceCode { get; set; }
    
    public Guid RootSalesClassId { get; set; }

    public Guid ClientProfileId { get; set; }
    
    public ClientProfileResponseDto ClientProfile { get; set; }
    
    public List<DetailSalesClassResponseDto> DetailSalesClasses { get; set; }
    
    public string? Icon { get; set; }

    public DateTime CreatedAt { get; set; }
}