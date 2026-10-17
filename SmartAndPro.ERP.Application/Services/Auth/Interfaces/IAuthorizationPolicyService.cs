using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

namespace SmartAndPro.ERP.Application.Services.Auth.Interfaces;

public interface IAuthorizationPolicyService
{
    Task<IEnumerable<AuthorizationPolicyDto>> GetAllPoliciesAsync();
}