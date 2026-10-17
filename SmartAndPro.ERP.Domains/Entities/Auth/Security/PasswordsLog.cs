namespace SmartAndPro.ERP.Domains.Entities.Auth.Security
{
    public class PasswordsLog
    {
        public Guid UserId { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
