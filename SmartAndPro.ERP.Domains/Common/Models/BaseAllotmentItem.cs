using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Domains.Common.Models
{
    public class BaseAllotmentItem<T>
    {
        public T Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public string ItemNo { get; set; }

        //public Guid MainPurchaseClassId { get; set; }
        //public MainPurchaseClass MainPurchaseClass { get; set; }
        public string? Name { get; set; }
        //public decimal Price { get; set; }
        //public decimal Quantity { get; set; } = 1;
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }

        // صافي الفاتورة بعد اضافة ضريبة
        public decimal TotalPrice { get; set; }

        // صافي الفاتورة من غير ضريبة
        public decimal NetPrice { get; set; }
    }
}
