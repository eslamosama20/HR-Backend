namespace SmartAndPro.ERP.Application.Common.Dtos.Return.Sale
{
    public class SalesReturnItemPostDto
    {
        public int? Id { get; set; }
        public string? ItemNo { get; set; }
        public Guid MainSalesClassId { get; set; }
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
        public int? SalesReturnId { get; set; }
    }
}
