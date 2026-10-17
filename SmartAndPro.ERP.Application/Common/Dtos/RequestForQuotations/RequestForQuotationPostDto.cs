using SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations.RequestForQuotationItems;
using SmartAndPro.ERP.Domains.Entities.RequestForQuotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing.Common;

namespace SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations
{
    public class RequestForQuotationPostDto
    {
        [MinLength(1, ErrorMessage = "You must add One item at least")]
        public List<RequestForQuotationItemPostDto> Items { get; set; }
        public virtual bool? IsActive { get; set; } = true;




    }
}
