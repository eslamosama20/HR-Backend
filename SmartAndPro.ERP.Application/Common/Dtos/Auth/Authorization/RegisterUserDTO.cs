namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

public class RegisterUserDTO
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    
    public string? WorkId { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string UserName { get; set; }

    public string Password { get; set; }
    
    public UserRoleDto[]? UserRoles { get; set; } = Array.Empty<UserRoleDto>();
    // public DateTime DateOfBirth { get; set; }
    public string? ProfilePicture { get; set; }

}