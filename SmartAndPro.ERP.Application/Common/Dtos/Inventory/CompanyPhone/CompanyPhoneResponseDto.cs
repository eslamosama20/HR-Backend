using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPhone
{
    public class CompanyPhoneResponseDto
    {
        public Guid Id { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsPrimary { get; set; }
    }
}
