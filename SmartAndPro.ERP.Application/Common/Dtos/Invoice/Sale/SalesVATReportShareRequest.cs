using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale
{
    public class SalesVATReportShareRequest
    {
        [JsonInclude]
        [Required(ErrorMessage = "StartDate Field is required")]
        public DateOnly StartDate { get; set; }
        
        [JsonInclude]
        [Required(ErrorMessage = "EndDate Field is required")]
        public DateOnly EndDate { get; set; }
        
        [JsonInclude]
        [Required(ErrorMessage = "Method Field is required")]
        public string Method { get; set; }
        
        [JsonInclude]
        [Required(ErrorMessage = "Receiver Field is required")]
        public string Receiver { get; set; }
        
        [JsonInclude]
        public string? Language { get; set; }
    }
}
