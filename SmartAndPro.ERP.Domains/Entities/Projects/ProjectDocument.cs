using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class ProjectDocument : Entity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty; // Assessment, PriceQuote, Authorization, Contract, etc.

        [MaxLength(500)]
        public string? DocumentName { get; set; }

        [MaxLength(1000)]
        public string? FilePath { get; set; }

        public long? FileSize { get; set; }

        [MaxLength(100)]
        public string? MimeType { get; set; }

        [MaxLength(100)]
        public string? UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string? MetaData { get; set; } // JSON for additional data

        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        [ForeignKey(nameof(ProjectId))]
        [JsonIgnore]
        public virtual Project Project { get; set; } = null!;
    }
}
