using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Common.Dtos.GeneralBankAccount
{
    public class GeneralBankAccountResponseDto
    {
        public  Guid Id { get; set; }

        public string? BankName { get; set; }

        public string? BankAccountNumber { get; set; }

        public string? AccountName { get; set; }

        public string? BankBranchName { get; set; }

        public string? SwiftCode { get; set; }
        public string? IBAN { get; set; }

        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid MainAccountId { get; set; }
        public bool IsFrontal { get; set; } = false;

    }
}
