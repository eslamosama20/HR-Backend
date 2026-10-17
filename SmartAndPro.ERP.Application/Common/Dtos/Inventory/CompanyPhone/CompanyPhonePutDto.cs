using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyPhone
{
    public class CompanyPhonePutDto
    {
        public Guid? Id { get; set; }

        public string? PhoneNumber { get; set; }

        public bool? IsPrimary { get; set; }
    }
}
