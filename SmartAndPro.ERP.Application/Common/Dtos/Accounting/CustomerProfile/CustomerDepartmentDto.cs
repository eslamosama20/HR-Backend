using System;

namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.CustomerProfile
{
    public class CustomerDepartmentDto
    {
        public Guid? Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string? NameEn { get; set; }
    }
}
