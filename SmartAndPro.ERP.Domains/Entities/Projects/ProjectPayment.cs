using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class ProjectPayment : Entity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        [MaxLength(100)]
        public string? PaymentMethod { get; set; } // Check, Transfer, Cash

        [MaxLength(200)]
        public string? PaymentReference { get; set; } // Check Number / Transfer Number

        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ProjectId))]
        [JsonIgnore]
        public virtual Project Project { get; set; } = null!;
    }
}
