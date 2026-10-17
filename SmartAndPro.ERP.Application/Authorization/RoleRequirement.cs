using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.Application.Authorization;

public class RoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }
    public RoleRequirement(string role)
    {
        Role = role;
    }
}