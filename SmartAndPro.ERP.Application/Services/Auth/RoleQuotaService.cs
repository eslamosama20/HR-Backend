using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.GeneralBankAccounts;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SupplierManagement;
using SmartAndPro.ERP.Domains.Entities.Treasuries;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.Application.Services.Auth;

public class RoleQuotaService : IRoleQuotaService
{
    private readonly ERPContext _db;

    public RoleQuotaService(ERPContext db)
    {
        _db = db;
    }

    private async Task<List<Role>> GetUserRolesAsync(ClaimsPrincipal userPrincipal)
    {
        if (userPrincipal == null) return new List<Role>();

        var userIdStr = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? userPrincipal.FindFirst(ClaimTypes.Name)?.Value
                     ?? userPrincipal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return new List<Role>();
        }

        var user = await _db.Set<User>()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.UserRoles == null) return new List<Role>();

        return user.UserRoles
            .Where(ur => ur.Role != null && ur.Role.IsActive)
            .Select(ur => ur.Role)
            .ToList();
    }

    private bool IsDevOrAdmin(List<Role> roles)
    {
        return roles.Any(r => r.Name == "System.Developer" || r.Name == "System.Admin");
    }

    public async Task ValidateCustomerQuotaAsync(ClaimsPrincipal userPrincipal)
    {
        var roles = await GetUserRolesAsync(userPrincipal);
        if (IsDevOrAdmin(roles)) return;

        var limits = roles
            .Where(r => r.MaxCustomersCount.HasValue)
            .Select(r => r.MaxCustomersCount.Value)
            .ToList();

        if (!limits.Any()) return;

        int maxAllowed = limits.Max();
        int currentCount = await _db.Set<CustomerProfile>().CountAsync(c => c.IsActive);

        if (currentCount >= maxAllowed)
        {
            throw new InvalidOperationException($"عفواً، لقد تجاوزت الحد الأقصى المسموح به لإنشاء العملاء لهذا الدور (الحد الأقصى: {maxAllowed}).");
        }
    }

    public async Task ValidateSupplierQuotaAsync(ClaimsPrincipal userPrincipal)
    {
        var roles = await GetUserRolesAsync(userPrincipal);
        if (IsDevOrAdmin(roles)) return;

        var limits = roles
            .Where(r => r.MaxSuppliersCount.HasValue)
            .Select(r => r.MaxSuppliersCount.Value)
            .ToList();

        if (!limits.Any()) return;

        int maxAllowed = limits.Max();
        int currentCount = await _db.Set<SupplierDetails>().CountAsync(s => s.IsActive);

        if (currentCount >= maxAllowed)
        {
            throw new InvalidOperationException($"عفواً، لقد تجاوزت الحد الأقصى المسموح به لإنشاء الموردين لهذا الدور (الحد الأقصى: {maxAllowed}).");
        }
    }

    public async Task ValidateBranchQuotaAsync(ClaimsPrincipal userPrincipal)
    {
        var roles = await GetUserRolesAsync(userPrincipal);
        if (IsDevOrAdmin(roles)) return;

        var limits = roles
            .Where(r => r.MaxBranchesCount.HasValue)
            .Select(r => r.MaxBranchesCount.Value)
            .ToList();

        if (!limits.Any()) return;

        int maxAllowed = limits.Max();
        int currentCount = await _db.Set<BranchDetails>().CountAsync(b => b.IsActive);

        if (currentCount >= maxAllowed)
        {
            throw new InvalidOperationException($"عفواً، لقد تجاوزت الحد الأقصى المسموح به لإنشاء الفروع لهذا الدور (الحد الأقصى: {maxAllowed}).");
        }
    }

    public async Task ValidateCompanyQuotaAsync(ClaimsPrincipal userPrincipal)
    {
        var roles = await GetUserRolesAsync(userPrincipal);
        if (IsDevOrAdmin(roles)) return;

        var limits = roles
            .Where(r => r.MaxCompaniesCount.HasValue)
            .Select(r => r.MaxCompaniesCount.Value)
            .ToList();

        if (!limits.Any()) return;

        int maxAllowed = limits.Max();
        int currentCount = await _db.Set<CompanyDetails>().CountAsync(c => c.IsActive);

        if (currentCount >= maxAllowed)
        {
            throw new InvalidOperationException($"عفواً، لقد تجاوزت الحد الأقصى المسموح به لإنشاء الشركات لهذا الدور (الحد الأقصى: {maxAllowed}).");
        }
    }

    public async Task ValidateStoreQuotaAsync(ClaimsPrincipal userPrincipal)
    {
        var roles = await GetUserRolesAsync(userPrincipal);
        if (IsDevOrAdmin(roles)) return;

        var limits = roles
            .Where(r => r.MaxStoresCount.HasValue)
            .Select(r => r.MaxStoresCount.Value)
            .ToList();

        if (!limits.Any()) return;

        int maxAllowed = limits.Max();
        int currentCount = await _db.Set<StoreDetails>().CountAsync(s => s.IsActive);

        if (currentCount >= maxAllowed)
        {
            throw new InvalidOperationException($"عفواً، لقد تجاوزت الحد الأقصى المسموح به لإنشاء المخازن لهذا الدور (الحد الأقصى: {maxAllowed}).");
        }
    }

    public async Task ValidateTreasuryQuotaAsync(ClaimsPrincipal userPrincipal)
    {
        var roles = await GetUserRolesAsync(userPrincipal);
        if (IsDevOrAdmin(roles)) return;

        var limits = roles
            .Where(r => r.MaxTreasuriesCount.HasValue)
            .Select(r => r.MaxTreasuriesCount.Value)
            .ToList();

        if (!limits.Any()) return;

        int maxAllowed = limits.Max();
        int currentCount = await _db.Set<Treasury>().CountAsync(t => t.IsActive);

        if (currentCount >= maxAllowed)
        {
            throw new InvalidOperationException($"عفواً، لقد تجاوزت الحد الأقصى المسموح به لإنشاء الخزائن لهذا الدور (الحد الأقصى: {maxAllowed}).");
        }
    }

    public async Task ValidateBankQuotaAsync(ClaimsPrincipal userPrincipal)
    {
        var roles = await GetUserRolesAsync(userPrincipal);
        if (IsDevOrAdmin(roles)) return;

        var limits = roles
            .Where(r => r.MaxBanksCount.HasValue)
            .Select(r => r.MaxBanksCount.Value)
            .ToList();

        if (!limits.Any()) return;

        int maxAllowed = limits.Max();
        int currentCount = await _db.Set<GeneralBankAccount>().CountAsync(b => b.IsActive);

        if (currentCount >= maxAllowed)
        {
            throw new InvalidOperationException($"عفواً، لقد تجاوزت الحد الأقصى المسموح به لإنشاء الحسابات البنكية لهذا الدور (الحد الأقصى: {maxAllowed}).");
        }
    }

    public async Task ValidateUserQuotaAsync(ClaimsPrincipal userPrincipal)
    {
        var roles = await GetUserRolesAsync(userPrincipal);
        if (IsDevOrAdmin(roles)) return;

        var limits = roles
            .Where(r => r.MaxUsersCount.HasValue)
            .Select(r => r.MaxUsersCount.Value)
            .ToList();

        if (!limits.Any()) return;

        int maxAllowed = limits.Max();
        int currentCount = await _db.Set<User>().CountAsync(u => u.IsActive);

        if (currentCount >= maxAllowed)
        {
            throw new InvalidOperationException($"عفواً، لقد تجاوزت الحد الأقصى المسموح به لإنشاء المستخدمين لهذا الدور (الحد الأقصى: {maxAllowed}).");
        }
    }
}
