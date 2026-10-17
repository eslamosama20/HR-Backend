using SmartAndPro.ERP.Application.Common.Dtos.Accounting.HeaderAccount;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.RootAccount;

public class RootAccountResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public string ReferenceCode { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public string? Icon { get; set; }
    
    public List<HeaderAccountResponseDto> HeaderAccounts { get; set; }
}