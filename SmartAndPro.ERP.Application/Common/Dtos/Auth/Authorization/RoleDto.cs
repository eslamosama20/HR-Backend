namespace SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;

public class RoleDto
{
    
    public Guid Id { get; set; }
    public string Name { get; set; }

    public Guid? DomainDetailsId { get; set; }
    
    public bool IsFrontal { get; set; } = false;
    public DomainDetailsDto? DomainDetails { get; set; }
    public List<RolePermissionDto>? RolePermissions { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
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