using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations.RequestForQuotationItems
{
    public class RequestForQuotationItemPostDto
    {
        [JsonInclude]
        [Required(ErrorMessage = "ItemName Field is required")] 
        public string ItemName { get; set; }
        [JsonInclude]
        [Required(ErrorMessage = "Quantity Field is required")]
        public decimal Quantity { get; set; }
        public virtual bool? IsActive { get; set; } = true;


    }
}
