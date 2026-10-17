using SmartAndPro.ERP.Application.Common.Dtos.Assets;
using SmartAndPro.ERP.Application.Common.Dtos.Bills.Accounts;

namespace SmartAndPro.ERP.Application.Common.Dtos.Bills.Exchange
{
    public class BillOfExchangeResponseDto
    {
        public Guid Id { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Description { get; set; }
        public string BillNo { get; set; }
        public string SellerId { get; set; }
        public Guid? TreasuryId { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? CreatedBy { get; set; }
        public DateTime BillOfExchangeCreatedAt { get; set; }
        public AssetResponseDto? Asset { get; set; }
        public string? AssetUrl { get; set; }
        public List<BillAccountsResponseDto> BillAccounts { get; set; }
    }
}
