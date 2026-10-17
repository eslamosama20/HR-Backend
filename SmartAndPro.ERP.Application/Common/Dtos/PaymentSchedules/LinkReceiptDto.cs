namespace SmartAndPro.ERP.Application.Common.Dtos.PaymentSchedules
{
    /// <summary>
    /// DTO لربط سند صرف بدفعة مجدولة
    /// </summary>
    public class LinkReceiptDto
    {
        /// <summary>معرف سند الصرف</summary>
        public Guid PaycheckReceiptId { get; set; }

        /// <summary>رقم السند (مرجع أو رقم تسلسلي)</summary>
        public string? ReceiptNo { get; set; }
    }
}
