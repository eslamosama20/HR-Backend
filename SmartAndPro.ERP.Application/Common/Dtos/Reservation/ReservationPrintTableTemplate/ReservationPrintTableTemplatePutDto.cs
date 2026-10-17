using SmartAndPro.ERP.Domains.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reservation.ReservationPrintTableTemplate;

public class ReservationPrintTableTemplatePutDto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }


    public List<PrintTableColumnDto> Columns { get; set; } = new List<PrintTableColumnDto>();
    public string PageOrientation { get; set; }
    public int DisplayOrder { get; set; }

}
