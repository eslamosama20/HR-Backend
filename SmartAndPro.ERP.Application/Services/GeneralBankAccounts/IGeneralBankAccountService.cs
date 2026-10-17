using SmartAndPro.ERP.Application.Common.Dtos.GeneralBankAccount;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.GeneralBankAccounts
{
    public interface IGeneralBankAccountService : IServiceBase
        <
        GeneralBankAccount,
        GeneralBankAccountPostDto,
        GeneralBankAccountPutDto,
        GeneralBankAccountResponseDto,
        GeneralBankAccountFiltersDto
        >
    {
        Task<OperationResponse<List<GeneralBankAccountResponseDto>>> GetAllFrontalAsync();
        Task<OperationResponse<GeneralBankAccountResponseDto>> MakeFrontalAsync(Guid generalBankAccountId);
        Task<GeneralBankAccountImportResultDto> ImportFromExcelAsync(Stream fileStream);

    }
}
