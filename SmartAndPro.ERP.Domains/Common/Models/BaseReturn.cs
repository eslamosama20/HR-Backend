using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Domains.Common.Models
{
    public class BaseReturn<T>
    {
        public T Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string ReturnNo { get; set; }
        public string Currency { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// تاريخ المطالبة
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// تاريخ الاستحقاق
        /// </summary>
        public DateTime DueDate { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal TotalPrice { get; set; }

        /// <summary>
        /// صافي الفاتورة بعد الخصم
        /// </summary>
        public decimal NetPrice { get; set; }


    }
}
