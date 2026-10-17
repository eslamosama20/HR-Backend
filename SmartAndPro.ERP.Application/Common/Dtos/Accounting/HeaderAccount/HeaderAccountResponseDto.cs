using SmartAndPro.ERP.Application.Common.Dtos.Accounting.ClientProfile;
using SmartAndPro.ERP.Application.Common.Dtos.Accounting.DetailAccount;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.HeaderAccount;

public class HeaderAccountResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public string ReferenceCode { get; set; }
    
    public Guid RootAccountId { get; set; }

    public Guid ClientProfileId { get; set; }
    
    public ClientProfileResponseDto ClientProfile { get; set; }
    
    public List<DetailAccountResponseDto> DetailAccounts { get; set; }
    
    public string? Icon { get; set; }

    public DateTime CreatedAt { get; set; }
}