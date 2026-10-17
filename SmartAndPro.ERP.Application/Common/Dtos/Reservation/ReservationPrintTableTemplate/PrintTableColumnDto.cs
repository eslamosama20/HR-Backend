using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationPrintTableTemplate;

public class PrintTableColumnDto
{
    [Required]
    [StringLength(100)]
    public string ColumnName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(50)]
    public string DataType { get; set; } = "string";

    public int OrderIndex { get; set; }

    public bool IsVisible { get; set; } = true;

    public int? Width { get; set; }

    [StringLength(50)]
    public string? Alignment { get; set; }
}
