using SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations.RequestForQuotationItems;
using SmartAndPro.ERP.Domains.Entities.RequestForQuotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations
{
    public class RequestForQuotationResponseDto
    {
        public Guid Id { get; set; }
        public List<RequestForQuotationItemResponseDto> Items { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual DateTime CreatedAt { get; set; }


    }
}
