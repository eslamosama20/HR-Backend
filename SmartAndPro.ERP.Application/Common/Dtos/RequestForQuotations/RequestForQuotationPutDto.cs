using SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations.RequestForQuotationItems;
using SmartAndPro.ERP.Domains.Entities.RequestForQuotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.RequestForQuotations
{
    public class RequestForQuotationPutDto
    {
        public Guid Id { get; set; }
        public List<RequestForQuotationItemPutDto> Items { get; set; }
        public virtual bool? IsActive { get; set; } = true;

    }
}
