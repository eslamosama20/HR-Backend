namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authentication;

public class VerifyOTPResponseDTO
{
/*"authToken": [],
"refreshToken": [],
"userRoles": [],
"expiresIn": []*/
    public string AuthToken { get; set; }
    public string RefreshToken { get; set; }
    public List<UserRoleResponseDTO> UserRoles { get; set; }
    public DateTime ExpiresIn { get; set; }
}