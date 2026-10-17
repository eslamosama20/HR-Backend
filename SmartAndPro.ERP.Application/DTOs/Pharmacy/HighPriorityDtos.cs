using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.DTOs.Pharmacy
{
    // ================== Insurance DTOs ==================
    
    public class InsuranceCheckRequest
    {
        [Required]
        public Guid InsuranceCompanyId { get; set; }
        
        [Required]
        public string MemberNo { get; set; } = string.Empty;
        
        [Required]
        public List<InsuranceItemDto> Items { get; set; } = new();
    }

    public class InsuranceItemDto
    {
        [Required]
        public Guid ItemId { get; set; }
        
        [Required]
        public decimal Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
    }

    public class InsuranceCheckResponse
    {
        public bool IsActive { get; set; }
        public string? RejectionReason { get; set; }
        public decimal CoveragePercentage { get; set; }
        public decimal PatientSharePercentage { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayableAmount { get; set; }
        public List<InsuranceItemCoverageDto> ItemsCoverage { get; set; } = new();
    }

    public class InsuranceItemCoverageDto
    {
        public Guid ItemId { get; set; }
        public bool IsCovered { get; set; }
        public decimal CoveragePercentage { get; set; }
        public decimal AmountCovered { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class InsuranceApprovalRequest
    {
        [Required]
        public Guid PrescriptionId { get; set; }
        
        [Required]
        public string ApprovalNo { get; set; } = string.Empty;
    }

    public class InsuranceApprovalResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // ================== Patient DTOs ==================
    
    public class PatientSearchQueryDto
    {
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PatientListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public DateTime? Dob { get; set; }
        public int Age { get; set; }
        public string? NationalId { get; set; }
        public bool HasInsurance { get; set; }
        public int PrescriptionsCount { get; set; }
    }

    public class PatientListResponseDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<PatientListDto> Items { get; set; } = new();
    }

    public class PatientDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Mobile { get; set; }
        public DateTime? Dob { get; set; }
        public int Age { get; set; }
        public string? Gender { get; set; }
        public string? NationalId { get; set; }
        public bool HasInsurance { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public List<PatientInsuranceDto> InsuranceInfo { get; set; } = new();
        public PatientHistoryDto History { get; set; } = new();
    }

    public class PatientInsuranceDto
    {
        public Guid InsuranceCompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string MemberNo { get; set; } = string.Empty;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
    }

    public class PatientHistoryDto
    {
        public int TotalPrescriptions { get; set; }
        public DateTime? LastVisit { get; set; }
        public List<string> KnownAllergies { get; set; } = new();
        public List<string> ChronicConditions { get; set; } = new();
    }

    public class CreatePatientRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Mobile { get; set; }
        
        public DateTime? Dob { get; set; }
        
        public string? Gender { get; set; }
        
        public string? NationalId { get; set; }
        
        public string? Address { get; set; }
        
        public string? Notes { get; set; }
    }

    public class CreatePatientResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // ================== Doctor DTOs ==================
    
    public class DoctorSearchQueryDto
    {
        public string? SearchTerm { get; set; }
        public string? Specialization { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class DoctorListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? LicenseNo { get; set; }
        public string? Mobile { get; set; }
        public bool IsActive { get; set; }
    }

    public class DoctorListResponseDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<DoctorListDto> Items { get; set; } = new();
    }

    // ================== Status Update DTOs ==================
    
    public class UpdatePrescriptionStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // review|dispensing|ready|delivered|cancelled
        
        public string? Notes { get; set; }
    }

    public class UpdatePrescriptionStatusResponse
    {
        public Guid Id { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    // ================== Alert & Interaction DTOs ==================
    
    public class DrugInteractionCheckRequest
    {
        [Required]
        public List<int> ItemIds { get; set; } = new();
        
        public int? PatientId { get; set; }
    }

    public class DrugInteractionCheckResponse
    {
        public bool HasInteractions { get; set; }
        public List<InteractionDto> Interactions { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class InteractionDto
    {
        public string Type { get; set; } = string.Empty; // drug-drug|drug-allergy|drug-condition
        public string Severity { get; set; } = string.Empty; // minor|moderate|major
        public string? Drug1 { get; set; }
        public string? Drug2 { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
    }
}
