using SmartAndPro.ERP.Application.Common.Dtos.Assets;
using SmartAndPro.ERP.Application.Common.Dtos.Bills.Accounts;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Bills.Exchange
{
    public class BillOfExchangePostDto
    {
        public decimal TotalPrice { get; set; }
        public string? Description { get; set; }
        public AssetPostDto? Asset { get; set; }
        public string BillNo { get; set; }
        public string SellerId { get; set; }
        public Guid? TreasuryId { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? CreatedBy { get; set; }
        public DateTime BillOfExchangeCreatedAt { get; set; }
        [MinLength(1)]
        public List<BillAccountPostDto> BillAccounts { get; set; }
    }
}
