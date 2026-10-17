using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainPurchaseClass;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.StoreDetails;

namespace SmartAndPro.ERP.Application.Common.Dtos.Return.Purchase
{
    public class PurchaseReturnItemDto
    {
        public int? Id { get; set; }
        public string? ItemNo { get; set; }
        public Guid MainPurchaseClassId { get; set; }
        public MainPurchaseClassResponseDto MainPurchaseClass { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// صافي الفاتورة بعد اضافة ضريبة
        /// </summary>
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// صافي الفاتورة من غير ضريبة
        /// </summary>
        public decimal NetPrice { get; set; }
        public int? PurchaseReturnId { get; set; }
        public Guid? StoreDetailsId { get; set; }
        public StoreDetailsResponseDto StoreDetails { get; set; }
    }
}
