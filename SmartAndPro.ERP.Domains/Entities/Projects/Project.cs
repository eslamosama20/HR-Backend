using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class Project : Entity
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string ProjectName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProjectNumber { get; set; }

        [MaxLength(100)]
        public string? FileNumber { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Active, Completed, Cancelled

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public bool IsDeleted { get; set; } = false;

        public bool IsFinancialSolvency { get; set; } = false;

        public decimal? TotalAmount { get; set; }

        public decimal? ExecutionAmount { get; set; }

        public decimal? ProfitPercentage { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CommissionPercentage { get; set; }

        [MaxLength(100)]
        public string? CheckNumber { get; set; }

        public DateTime? CheckReceiptDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CheckAmount { get; set; }

        public string? ClearingDetails { get; set; }

        public Guid? CompanyDetailsId { get; set; }

        [MaxLength(100)]
        public string? ShareToken { get; set; }

        [MaxLength(200)]
        public string? DepartmentName { get; set; }

        [MaxLength(200)]
        public string? SupervisingEntityName { get; set; }

        [MaxLength(200)]
        public string? ExecutingCompanyName { get; set; }

        [MaxLength(100)]
        public string? QuoteNumber { get; set; }

        public DateTime? QuoteDate { get; set; }

        [MaxLength(100)]
        public string? AssignmentNumber { get; set; }

        public DateTime? AssignmentApprovalDate { get; set; }

        public int? ExecutionDuration { get; set; }

        [MaxLength(50)]
        public string? ExecutionDurationUnit { get; set; }


        // Navigation Properties
        [ForeignKey(nameof(CompanyDetailsId))]
        public virtual CompanyDetails? CompanyDetails { get; set; }

        public virtual ProjectAssignmentPhase? AssignmentPhase { get; set; }
        public virtual ProjectExecutionPhase? ExecutionPhase { get; set; }
        public virtual ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
        public virtual ICollection<ProjectAssessment> Assessments { get; set; } = new List<ProjectAssessment>();
        public virtual ICollection<ProjectPriceQuote> PriceQuotes { get; set; } = new List<ProjectPriceQuote>();
        public virtual ICollection<ProjectPayment> Payments { get; set; } = new List<ProjectPayment>();
        public virtual ICollection<ProjectHistory> History { get; set; } = new List<ProjectHistory>();
        public virtual ICollection<ProjectExecutionMilestone> Milestones { get; set; } = new List<ProjectExecutionMilestone>();
    }
}
