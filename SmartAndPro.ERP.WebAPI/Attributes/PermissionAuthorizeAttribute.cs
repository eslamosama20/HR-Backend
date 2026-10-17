using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Attributes;

public class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    const string POLICY_PREFIX = "Permission";
    public PermissionAuthorizeAttribute(string permission) => Permission = permission;

    // Get or set the Permission property by manipulating the underlying Policy property
    public string Permission
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