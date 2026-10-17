using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.WebAPI.Controllers.Pharmacy.Dtos
{
    // =================== Insurance Management DTOs ===================
    
    /// <summary>
    /// Request to update insurance information for a prescription
    /// </summary>
    public class UpdateInsuranceRequest
    {
        public bool Enabled { get; set; }
        public int? InsuranceCompanyId { get; set; }
        public string? MemberNo { get; set; }
        public string? ApprovalNo { get; set; }
        
        [Range(0, 100)]
        public decimal? CoveragePercentage { get; set; }
        
        [Range(0, 100)]
        public decimal? PatientSharePercentage { get; set; }
        
        public string? Status { get; set; } // "pending", "approved", "rejected"
    }

    public class UpdateInsuranceResponse
    {
        public InsuranceDataDto Insurance { get; set; } = new();
        public FinancialSummaryDto Financials { get; set; } = new();
    }

    public class InsuranceDataDto
    {
        public bool Enabled { get; set; }
        public int? InsuranceCompanyId { get; set; }
        public string? InsuranceName { get; set; }
        public string? MemberNo { get; set; }
        public string? ApprovalNo { get; set; }
        public decimal CoveragePercentage { get; set; }
        public decimal PatientSharePercentage { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayableAmount { get; set; }
        public string? Status { get; set; }
    }

    public class FinancialSummaryDto
    {
        public decimal SubTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal InsuranceCovered { get; set; }
        public decimal PatientPayable { get; set; }
    }

    // =================== Prescription Actions DTOs ===================
    
    /// <summary>
    /// Request to mark prescription as ready
    /// </summary>
    public class MarkReadyRequest
    {
        public string? Notes { get; set; }
        public string? ConfirmedBy { get; set; }
    }

    public class MarkReadyResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "ready";
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request to send prescription to POS
    /// </summary>
    public class SendToPosRequest
    {
        [Required]
        public string PosTerminalId { get; set; } = "";
        public string? CashierUserId { get; set; }
    }

    public class SendToPosResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "sent_to_pos";
        public int? SalesInvoiceId { get; set; }
        public string? PosQueueNo { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request to cancel a prescription
    /// </summary>
    public class CancelPrescriptionRequest
    {
        [Required]
        public string Reason { get; set; } = "";
    }

    public class CancelPrescriptionResponse
    {
        public int Id { get; set; }
        public string Status { get; set; } = "cancelled";
        public string? CancelReason { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // =================== Attachments DTOs ===================
    
    public class AttachmentDto
    {
        public int AttachmentId { get; set; }
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long Size { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class UploadAttachmentResponse
    {
        public int AttachmentId { get; set; }
        public string FileName { get; set; } = "";
        public string ContentType { get; set; } = "";
        public long Size { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    // =================== Cashier DTOs ===================
    
    public class ReadyPrescriptionsQueryDto
    {
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ReadyPrescriptionItemDto
    {
        public int PrescriptionId { get; set; }
        public string RxNo { get; set; } = "";
        public string PatientName { get; set; } = "";
        public decimal GrandTotal { get; set; }
        public decimal PatientPayable { get; set; }
        public string Status { get; set; } = "ready";
        public DateTime? SentAt { get; set; }
    }

    public class ReadyPrescriptionsResponseDto
    {
        public int TotalCount { get; set; }
        public List<ReadyPrescriptionItemDto> Items { get; set; } = new();
    }

    // =================== Lookups DTOs ===================
    
    public class StoreLookupDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string? BranchId { get; set; }
    }

    public class StoresQueryDto
    {
        public string? BranchId { get; set; }
    }

    public class InsuranceCompanyLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal DefaultCoveragePercentage { get; set; }
    }

    public class InsuranceCompaniesQueryDto
    {
        public string? Term { get; set; }
        public int Take { get; set; } = 20;
    }
}
