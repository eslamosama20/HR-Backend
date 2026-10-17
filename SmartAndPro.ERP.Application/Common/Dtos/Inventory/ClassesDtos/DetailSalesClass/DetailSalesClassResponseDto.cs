using SmartAndPro.ERP.Application.Common.Dtos.Accounting.ClientProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.DetailSalesClass;

public class DetailSalesClassResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public string ReferenceCode { get; set; }
    
    public Guid HeaderSalesClassId { get; set; }
    public Guid ClientProfileId { get; set; }
    
    public ClientProfileResponseDto ClientProfile { get; set; }
    
    public string? Icon { get; set; }

    public bool IsForProjects { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public List<MainSalesClassResponseDto> MainSalesClasses { get; set; }
}