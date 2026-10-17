using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Treasuries
{
    public class TreasuryPutDto
    {
        [JsonInclude]
        [Required(ErrorMessage = "Id Field is required")]
        public Guid Id { get; set; }
        [JsonInclude]
        [Required(ErrorMessage = "Name Field is required")]
        public string Name { get; set; }

        [JsonInclude]
        [Required(ErrorMessage = "MainAccountId Field is required")]
        public Guid MainAccountId { get; set; }
        public bool IsActive { get; set; }
        public bool IsFrontal { get; set; } = false;
    }
}
