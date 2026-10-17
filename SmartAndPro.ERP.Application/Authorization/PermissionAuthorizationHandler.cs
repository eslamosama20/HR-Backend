using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;

namespace SmartAndPro.ERP.Application.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IRoleService _roleService; // A service to get role permissions
    private readonly IAuthService _authService;

    public PermissionAuthorizationHandler(IRoleService roleService, IAuthService authService)
    {
        _roleService = roleService;
        _authService = authService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User;
        if (user == null)
        {
            context.Fail();
            return;
        }

        // Check if the user has the required permission
        var userPermClaims = user.FindAll(c => c.Type == "Permission").Select(c => c.Value).ToList();
        if (CheckPermissionMatch(userPermClaims, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        // Check if any of the user's roles have the required permission
        if (user.HasClaim(c => c.Type == ClaimTypes.Role))
        {
            var userRoles = user.FindAll(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            foreach (var role in userRoles)
            {
                var rolePermissions = await _roleService.GetRolePermissionsAsync(role);
                var rolePermNames = rolePermissions.Select(p => p.Name).ToList();
                if (CheckPermissionMatch(rolePermNames, requirement.Permission))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
        
        var userId = context.User.FindFirst(ClaimTypes.Name)?.Value;
        if (userId != null)
        {
            var hasPermission = await _authService.AuthorizeAsync(Guid.Parse(userId), requirement.Permission);
            if (hasPermission)
            {
                context.Succeed(requirement);
                return;
            }
        }

        context.Fail();
    }

    private static bool CheckPermissionMatch(IEnumerable<string> userPerms, string required)
    {
        if (userPerms == null) return false;
        var permsList = userPerms as IList<string> ?? userPerms.ToList();
        if (permsList.Contains(required))
            return true;

        foreach (var p in permsList)
        {
            if (IsEquivalentOrImplied(p, required))
                return true;
        }

        return false;
    }

    private static bool IsEquivalentOrImplied(string userPerm, string required)
    {
        if (string.Equals(userPerm, required, StringComparison.OrdinalIgnoreCase))
            return true;

        var userParts = userPerm.Split('.');
        var reqParts = required.Split('.');

        string userEntity = userParts[0];
        string userAction = userParts.Length > 1 ? userParts[^1] : "";
        string reqEntity = reqParts[0];
        string reqAction = reqParts.Length > 1 ? reqParts[^1] : "";

        string normUserEntity = NormalizeEntity(userEntity);
        string normReqEntity = NormalizeEntity(reqEntity);

        if (normUserEntity.Equals(normReqEntity, StringComparison.OrdinalIgnoreCase))
        {
            if (userAction.Equals(reqAction, StringComparison.OrdinalIgnoreCase))
                return true;

            // Create, Update, Delete, Import, Export, Manage implies Read on the same entity
            if (reqAction.Equals("Read", StringComparison.OrdinalIgnoreCase) &&
                (userAction.Equals("Create", StringComparison.OrdinalIgnoreCase) ||
                 userAction.Equals("Update", StringComparison.OrdinalIgnoreCase) ||
                 userAction.Equals("Delete", StringComparison.OrdinalIgnoreCase) ||
                 userAction.Equals("Import", StringComparison.OrdinalIgnoreCase) ||
                 userAction.Equals("Export", StringComparison.OrdinalIgnoreCase) ||
                 userAction.Equals("Manage", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Import implies Create on the same entity
            if (reqAction.Equals("Create", StringComparison.OrdinalIgnoreCase) &&
                userAction.Equals("Import", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Sub-feature hierarchy
        if (userPerm.StartsWith("Projects.", StringComparison.OrdinalIgnoreCase) && required.Equals("Projects.Read", StringComparison.OrdinalIgnoreCase))
            return true;
        if (userPerm.StartsWith("Pharmacy.", StringComparison.OrdinalIgnoreCase) && required.Equals("Pharmacy.Read", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static string NormalizeEntity(string entity)
    {
        if (entity.StartsWith("Frontal", StringComparison.OrdinalIgnoreCase))
            entity = entity.Substring(7);
        if (entity.Equals("Branch", StringComparison.OrdinalIgnoreCase))
            return "BranchDetails";
        if (entity.Equals("InventoryManagement", StringComparison.OrdinalIgnoreCase))
            return "StoreDetails";
        if (entity.Equals("PurchasableReport", StringComparison.OrdinalIgnoreCase))
            return "PurchasableCardReport";
        if (entity.Equals("ReviewBalance", StringComparison.OrdinalIgnoreCase) || entity.Equals("ProfitLossAccount", StringComparison.OrdinalIgnoreCase) || entity.Equals("BudgetAssessment", StringComparison.OrdinalIgnoreCase))
            return "TrailBalance";
        return entity;
    }
}