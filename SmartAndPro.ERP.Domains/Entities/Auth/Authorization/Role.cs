using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.Entities.Auth.Authorization;

public class Role: Entity
{
    public string Name { get; set; }
    
    public Guid? DomainDetailsId { get; set; }
    public DomainDetails DomainDetails { get; set; }
    public List<RolePermission> RolePermissions { get; set; }
    
    public bool IsFrontal { get; set; } = false;
    public List<UserRole> UserRoles { get; set; }

    // قيود وترخيص الدور (Role Quotas)
    public int? MaxUsersCount { get; set; }
    public int? MaxCompaniesCount { get; set; }
    public int? MaxBranchesCount { get; set; }
    public int? MaxStoresCount { get; set; }
    public int? MaxBanksCount { get; set; }
    public int? MaxTreasuriesCount { get; set; }
    public int? MaxSuppliersCount { get; set; }
    public int? MaxCustomersCount { get; set; }
}