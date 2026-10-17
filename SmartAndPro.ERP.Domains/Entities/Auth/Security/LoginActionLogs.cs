namespace SmartAndPro.ERP.Domains.Entities.Auth.Security
{
    public class LoginActionLogs
    {
        public Guid UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public bool IsFailedTrail { get; set; }
    }
}
