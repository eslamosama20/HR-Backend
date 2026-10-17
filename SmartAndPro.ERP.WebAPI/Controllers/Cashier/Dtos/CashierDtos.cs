namespace SmartAndPro.ERP.WebAPI.Controllers.Cashier.Dtos
{
    /// <summary>
    /// POS Invoice Request - طلب إنشاء فاتورة نقطة بيع
    /// </summary>
    public class POSInvoiceRequest
    {
        public DateTime InvoiceDate { get; set; }
        public int? CashierSessionId { get; set; }
        public Guid? StoreDetailsId { get; set; }
        public int? CashierUserId { get; set; }
        
        // Customer Info
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerTaxNumber { get; set; }
        
        // Invoice Items
        public List<POSInvoiceItemRequest> Items { get; set; } = new();
        
        // Financial Details
        public decimal Subtotal { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Payment Details
        public int PaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal? CashAmount { get; set; }
        public decimal? CardAmount { get; set; }
        public decimal? OtherAmount { get; set; }
        public int? CreditDays { get; set; }
        
        // Additional Info
        public string? Notes { get; set; }
        public int? TreasuryId { get; set; }
        public int? BranchId { get; set; }
        public int? TableId { get; set; }
    }

    /// <summary>
    /// POS Invoice Item Request
    /// </summary>
    public class POSInvoiceItemRequest
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemNameAr { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int? UnitId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// POS Invoice Response
    /// </summary>
    public class POSInvoiceResponse
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
    }

    /// <summary>
    /// Payment Request - طلب معالجة الدفع
    /// </summary>
    public class PaymentRequest
    {
        public int InvoiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public int PaymentMethod { get; set; }
        public decimal? CashAmount { get; set; }
        public decimal? CardAmount { get; set; }
        public int? CreditDays { get; set; }
        public string? Notes { get; set; }
        /// <summary>Optional: Wasafaty prescription ID to auto-trigger dispense after payment</summary>
        public Guid? WasafatyPrescriptionId { get; set; }
        /// <summary>Optional: Local pharmacy prescription ID to auto-submit CCHI/NPHIES insurance claim after payment</summary>
        public Guid? PharmacyPrescriptionId { get; set; }
    }

    /// <summary>
    /// Payment Response
    /// </summary>
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public string? ReceiptUrl { get; set; }
    }

    /// <summary>
    /// Checkout Response (combined invoice + payment) - استجابة عملية البيع المجمعة
    /// </summary>
    public class CheckoutResponse
    {
        public bool Success { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public string? ZatcaQrCode { get; set; }
    }
}
