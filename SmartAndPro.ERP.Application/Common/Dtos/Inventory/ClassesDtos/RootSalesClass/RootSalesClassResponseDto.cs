using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.HeaderSalesClass;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.RootSalesClass;

public class RootSalesClassResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public string ReferenceCode { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public string? Icon { get; set; }
    
    public List<HeaderSalesClassResponseDto> HeaderSalesClasses { get; set; }
}