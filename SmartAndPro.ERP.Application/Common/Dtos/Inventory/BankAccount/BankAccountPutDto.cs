using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.BankAccount
{
    public class BankAccountPutDto
    {
        public Guid? Id { get; set; }
        public string? BankName { get; set; }

        public string? BankAccountNumber { get; set; }

        public string? AccountName { get; set; }

        public string? BankBranchName { get; set; }

        public string? SwiftCode { get; set; }
        public string? IBAN { get; set; }

        public bool? IsPrimary { get; set; }

    }
}
