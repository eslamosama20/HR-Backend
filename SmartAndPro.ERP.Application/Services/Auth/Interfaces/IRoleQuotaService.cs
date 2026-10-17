using System.Security.Claims;

namespace SmartAndPro.ERP.Application.Services.Auth.Interfaces;

public interface IRoleQuotaService
{
    Task ValidateCustomerQuotaAsync(ClaimsPrincipal userPrincipal);
    Task ValidateSupplierQuotaAsync(ClaimsPrincipal userPrincipal);
    Task ValidateBranchQuotaAsync(ClaimsPrincipal userPrincipal);
    Task ValidateCompanyQuotaAsync(ClaimsPrincipal userPrincipal);
    Task ValidateStoreQuotaAsync(ClaimsPrincipal userPrincipal);
    Task ValidateTreasuryQuotaAsync(ClaimsPrincipal userPrincipal);
    Task ValidateBankQuotaAsync(ClaimsPrincipal userPrincipal);
    Task ValidateUserQuotaAsync(ClaimsPrincipal userPrincipal);
}
