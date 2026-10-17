using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Authorization;

public class Permission : Entity
{
    public string Name { get; set; }
    
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    
    public Guid? DomainDetailsId { get; set; }
    public DomainDetails DomainDetails { get; set; }

}