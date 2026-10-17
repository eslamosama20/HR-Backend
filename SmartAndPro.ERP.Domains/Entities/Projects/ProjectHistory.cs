using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class ProjectHistory
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActionType { get; set; } = string.Empty; // Created, Updated, StatusChanged, etc.

        [MaxLength(100)]
        public string? ActionBy { get; set; }

        [Required]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; } = null!;
    }
}
