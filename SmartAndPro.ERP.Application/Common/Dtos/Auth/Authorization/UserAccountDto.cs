using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization
{
    public class UserAccountDto
    {
        public Guid Id { get; set; }
        
        public string? WorkId { get; set; }
        [JsonRequired]
        [Required(ErrorMessage = "Please enter a valid UserName")]
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        [JsonRequired]
        [Required(ErrorMessage = "Please enter a valid Email")]
        [EmailAddress]
        public string Email { get; set; }
        
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; }
        public bool IsFrontal { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool EmailVerified { get; set; }
        public List<UserRoleDto> UserRoles { get; set; }
        public string? ProfilePicture { get; set; }

    }

}
