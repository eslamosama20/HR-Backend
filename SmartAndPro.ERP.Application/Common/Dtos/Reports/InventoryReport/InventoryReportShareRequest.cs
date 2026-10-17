using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryStocks
{
    public class InventoryReportShareRequest
    {
        [Required(ErrorMessage = "Store ID is required")]
        public Guid? StoreId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Sharing method is required")]
        [StringLength(50, ErrorMessage = "Method cannot exceed 50 characters")]
        public string Method { get; set; } = string.Empty;

        [Required(ErrorMessage = "Receiver information is required")]
        [StringLength(100, ErrorMessage = "Receiver cannot exceed 100 characters")]
        public string Receiver { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Language cannot exceed 10 characters")]
        public string? Language { get; set; } = "ar";
    }
}