using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authentication;

public class UserRoleResponseDTO
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public RoleDto Role { get; set; }
}