using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;

public class ProductBatchToUnitAssignmentsDto
{
    [JsonInclude]
    [Required(ErrorMessage = "ProductUnitId Field is required")]
    public Guid? ProductUnitId { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "BatchUnitStatusId Field is required")]
    public Guid? BatchUnitStatusId { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "ProductBatchDetailsId Field is required")]
    public Guid? ProductBatchDetailsId { get; set; }

    public bool? IsActive { get; set; } = true;


    public Guid? CreatedBy { get; set; }
}