using SmartAndPro.ERP.Application.Common.Dtos.Bills.Accounts;
using SmartAndPro.ERP.Domains.Entities.Assets;

namespace SmartAndPro.ERP.Application.Common.Dtos.Bills.Sale
{
    public class BillOfSaleDto
    {
        public Guid Id { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Description { get; set; }
        public string BillNo { get; set; }
        public string SellerId { get; set; }
        public Guid? TreasuryId { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? CreatedBy { get; set; }
        public DateTime BillOfSaleCreatedAt { get; set; }
        public Guid? AssetId { get; set; }
        public Asset? Asset { get; set; }
        public List<BillAccountsDto> BillAccounts { get; set; }
    }
}
