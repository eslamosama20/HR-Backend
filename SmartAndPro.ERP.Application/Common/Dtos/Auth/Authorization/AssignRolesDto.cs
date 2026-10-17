namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

public class AssignRolesDto
{
    public Guid UserId { get; set; }
    public List<Guid> RoleIds { get; set; }
}