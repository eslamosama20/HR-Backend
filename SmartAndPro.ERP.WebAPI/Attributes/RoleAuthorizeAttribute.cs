using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Attributes;

public class RoleAuthorizeAttribute : AuthorizeAttribute
{
    const string POLICY_PREFIX = "Role";
    public RoleAuthorizeAttribute(string role) => Role = role;

    // Get or set the Role property by manipulating the underlying Policy property
    public string Role
    {
        get
        {
            return Policy.Substring(POLICY_PREFIX.Length + 1);
        }
        set
        {
            Policy = $"{POLICY_PREFIX}.{value}";
        }
    }
}
