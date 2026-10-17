using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ClassesDtos.MainSalesClass;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class SaleItemDto
    {
        public int? Id { get; set; }
        public string? ItemNo { get; set; }
        public Guid MainSalesClassId { get; set; }
        public MainSalesClassResponseDto MainSalesClass { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public int NoOfNights { get; set; } = 1;


        /// <summary>
        /// صافي الفاتورة بعد اضافة ضريبة
        /// </summary>
        public decimal TotalPrice { get; set; }
        /// <summary>
        /// صافي الفاتورة من غير ضريبة
        /// </summary>
        public decimal NetPrice { get; set; }
        public int? SaleInvoiceId { get; set; }
        public decimal ReturnedQuantity { get; set; }
    }
}
