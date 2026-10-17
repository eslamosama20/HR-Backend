using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.GeneralBankAccount
{
    public class GeneralBankAccountPostDto
    {
        [JsonInclude]
        [Required(ErrorMessage = "BankName Field is required")]
        public string BankName { get; set; }

        [JsonInclude]
        [Required(ErrorMessage = "BankAccountNumber Field is required")]
        public string BankAccountNumber { get; set; }

        [JsonInclude]
        [Required(ErrorMessage = "AccountName Field is required")]
        public string AccountName { get; set; }

        [JsonInclude]
        [Required(ErrorMessage = "BankBranchName Field is required")]
        public string BankBranchName { get; set; }

        public string? SwiftCode { get; set; }
        public string? IBAN { get; set; }

        [JsonInclude]
        [Required(ErrorMessage = "MainAccountId Field is required")]
        public Guid MainAccountId { get; set; }
        public bool IsFrontal { get; set; } = false;
    }
}
