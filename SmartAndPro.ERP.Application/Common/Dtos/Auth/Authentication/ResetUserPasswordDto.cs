namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authentication;

public class ResetUserPasswordDto
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; }
}
