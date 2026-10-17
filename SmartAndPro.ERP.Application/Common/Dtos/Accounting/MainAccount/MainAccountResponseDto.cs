using SmartAndPro.ERP.Application.Common.Dtos.Accounting.ClientProfile;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

public class MainAccountResponseDto
{
    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public string ReferenceCode { get; set; }
    
    public Guid DetailAccountId { get; set; }
    public Guid ClientProfileId { get; set; }
    
    public ClientProfileResponseDto ClientProfile { get; set; }
    
    public string? Icon { get; set; }

    public DateTime CreatedAt { get; set; }
}