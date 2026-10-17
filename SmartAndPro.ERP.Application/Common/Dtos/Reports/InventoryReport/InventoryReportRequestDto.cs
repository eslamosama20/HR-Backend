using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryReport
{
    public class InventoryReportRequestDto
    {

        [JsonInclude]
        [Required(ErrorMessage = "Report StartDate Field is required")]
        public DateTime? StartDate { get; set; }

        [JsonInclude]
        [Required(ErrorMessage = "Report EndDate Field is required")]
        public DateTime? EndDate { get; set; }

        [JsonInclude]
        [Required(ErrorMessage = "Inventory Id Field is required")]
        public Guid Id { get; set; }
    }
}
