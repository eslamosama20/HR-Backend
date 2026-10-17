using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BatchDetails;

public class BatchDetailsPutDto
{
    [JsonInclude]
    [Required(ErrorMessage = "Id Field is required")]
    public Guid? Id { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "ExpiryDate Field is required")]
    public DateTime? ExpiryDate { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "BatchStatusId Field is required")]
    public Guid? BatchStatusId { get; set; }

    [JsonInclude]
    [Required(ErrorMessage = "StoreDetailsId Field is required")]
    public Guid? StoreDetailsId { get; set; }

    public bool? IsActive { get; set; } = true;
}