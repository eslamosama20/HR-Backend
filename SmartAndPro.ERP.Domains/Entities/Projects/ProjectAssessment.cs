using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class ProjectAssessment : Entity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public DateTime AssessmentDate { get; set; }

        [MaxLength(200)]
        public string? AssessedBy { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AssessmentAmount { get; set; }

        public string? AssessmentNotes { get; set; }

        public Guid? DocumentId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey(nameof(DocumentId))]
        public virtual ProjectDocument? Document { get; set; }
    }
}
