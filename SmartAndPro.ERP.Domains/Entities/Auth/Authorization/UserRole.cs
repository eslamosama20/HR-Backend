using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Authorization;

public class UserRole : Entity
{
    public Guid? UserId { get; set; }
    public Guid RoleId { get; set; }
    public User? User { get; set; }
    public Role? Role { get; set; }
}