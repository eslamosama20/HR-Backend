namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization
{
    public class UserDTO
    {
        public string authToken { get; set; }
        public string refreshToken { get; set; }
        
        public List<UserRoleDto> UserRoles { get; set; }
        public DateTime expiresIn { get; set; }
    }

}
