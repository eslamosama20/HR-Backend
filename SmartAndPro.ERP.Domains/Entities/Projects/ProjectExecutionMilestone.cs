using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class ProjectExecutionMilestone : Entity
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(500)]
        public string MilestoneName { get; set; } = string.Empty;

        public int Duration { get; set; }

        [MaxLength(50)]
        public string DurationUnit { get; set; } = "Month"; // Day, Month, Year

        public int SequenceOrder { get; set; } // الترتيب التسلسلي للمرحلة

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletionDate { get; set; }

        public DateTime? TargetStartDate { get; set; }
        public DateTime? TargetEndDate { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ProjectId))]
        [JsonIgnore]
        public virtual Project Project { get; set; } = null!;
    }
}
