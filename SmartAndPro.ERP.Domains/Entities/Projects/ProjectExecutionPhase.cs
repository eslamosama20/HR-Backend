using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class ProjectExecutionPhase : Entity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GrantAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ExecutionAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RemainingAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? CompletionPercentage { get; set; }

        public bool TransferIssued { get; set; } = false;

        public DateTime? TransferDate { get; set; }

        public bool ContractDataSent { get; set; } = false;

        public DateTime? ContractSentDate { get; set; }

        public bool FinalReportSent { get; set; } = false;

        public DateTime? FinalReportDate { get; set; }

        public Guid? CompanyId { get; set; }

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ProjectId))]
        [JsonIgnore]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey(nameof(CompanyId))]
        public virtual Company? Company { get; set; }
    }
}
