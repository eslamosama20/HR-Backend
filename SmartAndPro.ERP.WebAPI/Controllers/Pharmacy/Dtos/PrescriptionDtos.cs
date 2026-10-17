using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.WebAPI.Controllers.Pharmacy.Dtos
{
    // ================== List & Filtering DTOs ==================
    
    public class PrescriptionListQueryDto
    {
        public string? Status { get; set; } // new|review|dispensing|ready|delivered|cancelled
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Priority { get; set; } // normal|urgent
        public string? PaymentType { get; set; } // cash|insurance
    }

    public class PrescriptionListItemDto
    {
        public Guid Id { get; set; }
        public string RxNo { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? DoctorName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string? InsuranceName { get; set; }
        public int ItemsCount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PatientPayable { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PrescriptionListResponseDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<PrescriptionListItemDto> Items { get; set; } = new();
    }

    // ================== Details DTOs ==================
    
    public class PrescriptionDetailsDto
    {
        public Guid Id { get; set; }
        public string RxNo { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public PatientDto Patient { get; set; } = new();
        public DoctorDto? Doctor { get; set; }
        public BranchDto Branch { get; set; } = new();
        public StoreDto Store { get; set; } = new();
        public string PaymentType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public InsuranceDto? Insurance { get; set; }
        public List<PrescriptionItemDto> Items { get; set; } = new();
        public List<AttachmentDto> Attachments { get; set; } = new();
        public List<AlertDto> Alerts { get; set; } = new();
        public FinancialsDto Financials { get; set; } = new();
    }

    public class PatientDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public DateTime? Dob { get; set; }
    }

    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LicenseNo { get; set; }
    }

    public class BranchDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class StoreDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class InsuranceDto
    {
        public bool Enabled { get; set; }
        public Guid? InsuranceCompanyId { get; set; }
        public string? InsuranceName { get; set; }
        public string? MemberNo { get; set; }
        public string? ApprovalNo { get; set; }
        public decimal CoveragePercentage { get; set; }
        public decimal PatientSharePercentage { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayableAmount { get; set; }
        public string Status { get; set; } = "pending"; // pending|approved|rejected
    }

    public class PrescriptionItemDto
    {
        public Guid LineId { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public decimal DispensedQuantity { get; set; }
        public string? StoreId { get; set; }
        public string? BatchId { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool AllowAlternative { get; set; }
        public bool IsControlled { get; set; }
        public string? Notes { get; set; }
        public string AvailabilityStatus { get; set; } = "available"; // available|partial|unavailable
    }

    public class AlertDto
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // info|warning|error
        public string Message { get; set; } = string.Empty;
    }

    public class FinancialsDto
    {
        public decimal SubTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal InsuranceCovered { get; set; }
        public decimal PatientPayable { get; set; }
    }

    // ================== Create/Update DTOs ==================
    
    public class CreatePrescriptionRequest
    {
        [Required]
        public string RxNo { get; set; } = string.Empty;
        
        [Required]
        public DateTime IssueDate { get; set; }
        
        [Required]
        public Guid PatientId { get; set; }
        
        public Guid? DoctorId { get; set; }
        
        [Required]
        public string BranchId { get; set; } = string.Empty;
        
        [Required]
        public string StoreId { get; set; } = string.Empty;
        
        [Required]
        public string PaymentType { get; set; } = "cash"; // cash|insurance
        
        public string Priority { get; set; } = "normal"; // normal|urgent
        
        public string? Notes { get; set; }
        
        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreatePrescriptionItemRequest> Items { get; set; } = new();
    }

    public class CreatePrescriptionItemRequest
    {
        [Required]
        public Guid ItemId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal RequestedQuantity { get; set; }
        
        public decimal DispensedQuantity { get; set; } = 0;
        
        [Required]
        public string StoreId { get; set; } = string.Empty;
        
        public string? BatchId { get; set; }
        
        public bool AllowAlternative { get; set; } = false;
        
        public string? Notes { get; set; }
    }

    public class CreatePrescriptionResponse
    {
        public Guid Id { get; set; }
        public string RxNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdatePrescriptionRequest
    {
        [Required]
        public string RxNo { get; set; } = string.Empty;
        
        [Required]
        public DateTime IssueDate { get; set; }
        
        [Required]
        public Guid PatientId { get; set; }
        
        public Guid? DoctorId { get; set; }
        
        [Required]
        public string BranchId { get; set; } = string.Empty;
        
        [Required]
        public string StoreId { get; set; } = string.Empty;
        
        [Required]
        public string PaymentType { get; set; } = "cash";
        
        public string Priority { get; set; } = "normal";
        
        public string? Notes { get; set; }
        
        [Required]
        [MinLength(1)]
        public List<CreatePrescriptionItemRequest> Items { get; set; } = new();
        
        public string? Status { get; set; }
        
        public InsuranceUpdateDto? Insurance { get; set; }
    }

    public class InsuranceUpdateDto
    {
        public bool Enabled { get; set; }
    }

    public class UpdatePrescriptionResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
