using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;

public class UnassignProductFromBatchDto
{
    [JsonInclude]
    [Required(ErrorMessage = "productUnitId Field is required")]
    public Guid productUnitId { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "batchId Field is required")]
    public Guid batchDetailsId { get; set; }
}