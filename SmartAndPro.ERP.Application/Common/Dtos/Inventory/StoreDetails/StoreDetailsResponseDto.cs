using SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreStatus;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;

public class StoreDetailsResponseDto
{
    public Guid Id { get; set; }

    public string ArabicName { get; set; }

    public string EnglishName { get; set; }
    
    public Guid? MainAccountId { get; set; }
    public MainAccountResponseDto MainAccount { get; set; }


    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid CompanyId { get; set; }
    public StoreStatusResponseDto StoreStatus { get; set; }
    public Guid StoreStatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string StoreType { get; set; }
    public string? StoreCategory { get; set; }
}