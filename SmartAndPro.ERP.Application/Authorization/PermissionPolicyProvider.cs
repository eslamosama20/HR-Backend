using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SmartAndPro.ERP.Application.Authorization;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PermissionPrefix = "Permission.";
    private const string RolePrefix = "Role.";
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring(PermissionPrefix.Length);
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(permission));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        if (policyName.StartsWith(RolePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var role = policyName.Substring(RolePrefix.Length);
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new RoleRequirement(role));
            return Task.FromResult<AuthorizationPolicy?>(policy.Build());
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
