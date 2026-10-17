using System;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects
{
    public class ProjectCheckDto
    {
        public Guid? Id { get; set; } // Matches the PaycheckReceiptId
        public string CheckNumber { get; set; } = string.Empty;
        public DateTime CheckReceiptDate { get; set; }
        public decimal CheckAmount { get; set; }
        public Guid TreasuryId { get; set; } // The target treasury/bank
        public string? TreasuryName { get; set; }
        public Guid MainAccountId { get; set; } // The credited account (الطرف الدائن)
        public string? MainAccountName { get; set; }
        public int? PaymentMethodType { get; set; }
        public string? BankName { get; set; }
        public string? BankReferenceNumber { get; set; }
        public string? VoucherNumber { get; set; }
        public string? TransactionNumber { get; set; }
        public string? CheckNumberDetail { get; set; }
    }
}
