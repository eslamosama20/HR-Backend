using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class ProjectAssignmentPhase : Entity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        public Guid? ExecutingEntityId { get; set; }

        [MaxLength(200)]
        public string? ExecutingEntityName { get; set; }

        public Guid? SupervisingEntityId { get; set; }

        [MaxLength(200)]
        public string? SupervisingEntityName { get; set; }

        public Guid? DepartmentId { get; set; }

        [MaxLength(200)]
        public string? DepartmentName { get; set; }

        public DateTime? AssignmentDate { get; set; }

        public DateTime? AssignmentApprovalDate { get; set; }

        public DateTime? ExecutionStartDate { get; set; }

        public int? ExecutionDuration { get; set; }

        [MaxLength(50)]
        public string? ExecutionDurationUnit { get; set; } // Day, Month, Year

        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ProjectId))]
        [JsonIgnore]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey(nameof(ExecutingEntityId))]
        public virtual Company? ExecutingEntity { get; set; }

        [ForeignKey(nameof(SupervisingEntityId))]
        public virtual Company? SupervisingEntity { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }
    }
}
