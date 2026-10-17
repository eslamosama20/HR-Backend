using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class Department : Entity
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? DepartmentCode { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<ProjectAssignmentPhase> Projects { get; set; } = new List<ProjectAssignmentPhase>();
    }
}
