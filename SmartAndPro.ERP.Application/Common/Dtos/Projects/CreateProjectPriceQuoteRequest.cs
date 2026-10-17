using System;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Projects;

public class CreateProjectPriceQuoteRequest
{
    [Required(ErrorMessage = "معرف المشروع مطلوب")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "تاريخ عرض السعر مطلوب")]
    public DateTime QuoteDate { get; set; }

    [MaxLength(100, ErrorMessage = "رقم العرض لا يمكن أن يتجاوز 100 حرف")]
    public string? QuoteNumber { get; set; }

    [Required(ErrorMessage = "قيمة العرض مطلوبة")]
    [Range(0.01, double.MaxValue, ErrorMessage = "قيمة العرض يجب أن تكون أكبر من صفر")]
    public decimal QuoteAmount { get; set; }

    public DateTime? ValidUntil { get; set; }

    public Guid? CompanyId { get; set; }

    public Guid? DocumentId { get; set; }

    public int? SaleInvoiceId { get; set; }
}

public class UpdateProjectPriceQuoteRequest : CreateProjectPriceQuoteRequest
{
    [Required(ErrorMessage = "معرف عرض السعر مطلوب")]
    public Guid Id { get; set; }
}
