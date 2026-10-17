namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

public class AuthorizationPolicyDto
{
    
    public string Name { get; set; }
    public string Value { get; set; }
    // type "Permission" or "Role" only
    public PolicyType Type { get; set; }
}

public enum PolicyType
{
    Permission,
    Role
}