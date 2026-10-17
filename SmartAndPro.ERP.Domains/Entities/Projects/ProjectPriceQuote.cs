using SmartAndPro.ERP.Domains.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartAndPro.ERP.Domains.Entities.Projects
{
    public class ProjectPriceQuote : Entity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public DateTime QuoteDate { get; set; }

        [MaxLength(100)]
        public string? QuoteNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? QuoteAmount { get; set; }

        public DateTime? ValidUntil { get; set; }

        public Guid? CompanyId { get; set; }

        public Guid? DocumentId { get; set; }

        public bool IsAccepted { get; set; } = false;

        public int? SaleInvoiceId { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(SaleInvoiceId))]
        public virtual SmartAndPro.ERP.Domains.Entities.Invoice.Sale.SaleInvoice? SaleInvoice { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey(nameof(CompanyId))]
        public virtual Company? Company { get; set; }

        [ForeignKey(nameof(DocumentId))]
        public virtual ProjectDocument? Document { get; set; }
    }
}
