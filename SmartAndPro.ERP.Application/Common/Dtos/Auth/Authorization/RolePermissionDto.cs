namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

public class RolePermissionDto
{
    public Guid? RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public PermissionDto? Permission { get; set; }
}