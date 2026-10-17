using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;

namespace SmartAndPro.ERP.Domains.Common.Models
{
    public class BaseSalesReturnItem<T>
    {
        public T Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ItemNo { get; set; }
        public Guid MainSalesClassId { get; set; }
        public MainSalesClass MainSalesClass { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; } =1;
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
    }
}
