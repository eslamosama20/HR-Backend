using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Notifications;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Security;

public class User
{
    public Guid Id { get; set; }
    public string? WorkId { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }

    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string Password { get; set; }
    public bool TwoFactorEnabled { get; set; } = false;
    public bool EmailVerified { get; set; } = false;
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public bool IsFrontal { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public Guid? CompanyDetailsId { get; set; }
    
    public List<UserRole> UserRoles { get; set; }

    public byte[]? ProfilePicture { get; set; }

    public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();


}