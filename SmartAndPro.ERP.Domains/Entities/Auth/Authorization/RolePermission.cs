using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Authorization;

public class RolePermission : Entity
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; }
}