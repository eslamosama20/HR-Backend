using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.Application.Authorization;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        var user = context.User;
        if (user == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }
        
        // Check if the user has the required role
        if (user.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == requirement.Role))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}