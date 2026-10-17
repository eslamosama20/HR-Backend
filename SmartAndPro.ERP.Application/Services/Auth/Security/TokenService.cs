using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartAndPro.ERP.Application.Services.Auth.Abstraction;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;

namespace SmartAndPro.ERP.Application.Services.Auth.Security
{
    public class TokenService : BaseService
    {
        private readonly SymmetricSecurityKey key;
        private readonly TokenSettings Token;
        private readonly IMapper _mapper;
        private readonly RepositoriesFactory _repositoriesFactory;
        public TokenService(RepositoriesFactory repositoriesFactory,
        AppSettings appSettings, IMapper mapper) : base(repositoriesFactory, appSettings)
        {
            _repositoriesFactory = repositoriesFactory;
            Token = appSettings.Token;
            key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Token.Key));
            _mapper = mapper;
        }

        public async Task<string> CreateToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var userRoles = user.UserRoles;
            if (userRoles == null || userRoles.Count == 0 || userRoles.Any(r => r.Role == null))
            {
                var userRolesQuery = await _repositoriesFactory
                    .GetRepository<UserRole>()
                    .Filter(ur => ur.UserId == user.Id);

                var query = userRolesQuery.Data;
                userRoles = query is IAsyncEnumerable<UserRole>
                    ? await query.ToListAsync()
                    : query.ToList();

                if (userRoles.Any(ur => ur.Role == null))
                {
                    var roleIds = userRoles
                        .Select(ur => ur.RoleId)
                        .Distinct()
                        .ToList();

                    if (roleIds.Count > 0)
                    {
                        var rolesQuery = await _repositoriesFactory
                            .GetRepository<Role>()
                            .Filter(role => roleIds.Contains(role.Id));

                        var rolesData = rolesQuery.Data;
                        var roles = rolesData is IAsyncEnumerable<Role>
                            ? await rolesData.ToListAsync()
                            : rolesData.ToList();

                        var roleById = roles.ToDictionary(role => role.Id);
                        foreach (var userRole in userRoles)
                        {
                            if (userRole.Role == null && roleById.TryGetValue(userRole.RoleId, out var role))
                            {
                                userRole.Role = role;
                            }
                        }
                    }
                }
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Name ,user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? user.UserName ?? ""),
                new Claim(ClaimTypes.Surname, user.LastName ?? ""),
            };

            var roleNames = userRoles?
                .Select(userRole => userRole?.Role?.Name?.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            var hasPosPermission = false;
            try
            {
                var roleIds = userRoles?.Select(ur => ur.RoleId).ToList() ?? new List<Guid>();
                if (roleIds.Count > 0)
                {
                    var rolePermsRepo = _repositoriesFactory.GetRepository<RolePermission>();
                    var rolePermsQuery = await rolePermsRepo.Filter(rp => roleIds.Contains(rp.RoleId));
                    var rolePerms = rolePermsQuery.Data?.ToList() ?? new List<RolePermission>();
                    var permissionIds = rolePerms.Select(rp => rp.PermissionId).Distinct().ToList();
                    
                    if (permissionIds.Count > 0)
                    {
                        var permsRepo = _repositoriesFactory.GetRepository<Permission>();
                        var permsQuery = await permsRepo.Filter(p => permissionIds.Contains(p.Id));
                        var perms = permsQuery.Data?.ToList() ?? new List<Permission>();
                        hasPosPermission = perms.Any(p => p.Name == "Cashier.POS");
                    }
                }
            }
            catch (Exception)
            {
                // Fallback to false
            }

            foreach (var roleName in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName!));

                var normalized = roleName!.ToLowerInvariant();
                if (normalized.Contains("admin") || normalized.Contains("مدير"))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }

                if (normalized.Contains("cashier") || normalized.Contains("كاشير"))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Cashier"));
                }
            }

            if (hasPosPermission && !claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Cashier"))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Cashier"));
            }
            

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken token = new JwtSecurityToken(issuer: Token.Issuer,
                                                          expires: DateTime.UtcNow.AddDays(double.Parse(Token.DurationInDays)),
                                                          claims: claims,
                                                          signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
