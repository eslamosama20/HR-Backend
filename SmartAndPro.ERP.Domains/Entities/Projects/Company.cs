using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class Company : Entity
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CompanyType { get; set; } // Executing, Supervising, etc.

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<ProjectAssignmentPhase> ExecutingProjects { get; set; } = new List<ProjectAssignmentPhase>();
        public virtual ICollection<ProjectAssignmentPhase> SupervisingProjects { get; set; } = new List<ProjectAssignmentPhase>();
        public virtual ICollection<ProjectExecutionPhase> ExecutionPhases { get; set; } = new List<ProjectExecutionPhase>();
        public virtual ICollection<ProjectPriceQuote> PriceQuotes { get; set; } = new List<ProjectPriceQuote>();
    }
}
