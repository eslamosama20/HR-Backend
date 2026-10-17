using SmartAndPro.ERP.Domains.Enums;
using System;
using System.Collections.Generic;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationPrintTableTemplate;

public class ReservationPrintTableTemplateResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<PrintTableColumnDto> Columns { get; set; } = new List<PrintTableColumnDto>();
    public DateTime CreatedAt { get; set; }
    public string PageOrientation { get; set; }
    public int DisplayOrder { get; set; }

}
