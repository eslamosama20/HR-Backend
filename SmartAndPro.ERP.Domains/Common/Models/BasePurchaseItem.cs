using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;

namespace SmartAndPro.ERP.Domains.Common.Models
{
    public class BasePurchaseItem<T>
    {
        public T Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ItemNo { get; set; }
        public Guid MainPurchaseClassId { get; set; }
        public MainPurchaseClass MainPurchaseClass { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }=1;
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool IsTaxExempted { get; set; } = false;

        /// <summary>
        /// صافي الفاتورة بعد اضافة ضريبة
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// صافي الفاتورة من غير ضريبة
        /// </summary>
        public decimal NetPrice { get; set; }
    }
}
