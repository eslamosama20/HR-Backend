using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Auth.Security;

[Route("api/auth/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly ServicesFactory _serviceFactory;
    private readonly TokenSettings _token;
    private readonly ERPContext _context;


    public UserController(ServicesFactory serviceFactory,
                             RepositoriesFactory repositoriesFactory,
                             IMapper mapper,
                             AppSettings appSettings,
                             ERPContext context)
    {
        _token = appSettings.Token;
        _serviceFactory = serviceFactory;
        this._repositoriesFactory = repositoriesFactory;
        this._mapper = mapper;
        _context = context;
    }
    
    private bool IsCurrentUserDeveloper()
    {
        if (User.IsInRole("System.Developer") || 
            User.Claims.Any(c => c.Type == System.Security.Claims.ClaimTypes.Role && (c.Value == "System.Developer" || c.Value == "Developer")))
        {
            return true;
        }

        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email) && (email.Equals("developer@smartapro.com", StringComparison.OrdinalIgnoreCase) || email.Equals("smaartx7@gmail.com", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var givenName = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value;
        if (!string.IsNullOrEmpty(givenName) && (givenName.Equals("sys_developer", StringComparison.OrdinalIgnoreCase) || givenName.Equals("developer", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }

    private bool IsCurrentUserAdmin()
    {
        if (User.IsInRole("System.Admin") || 
            User.Claims.Any(c => c.Type == System.Security.Claims.ClaimTypes.Role && (c.Value == "System.Admin" || c.Value == "Admin")))
        {
            return true;
        }

        var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email) && email.Equals("admin@admin.com", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var givenName = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.GivenName)?.Value;
        if (!string.IsNullOrEmpty(givenName) && (givenName.Equals("admin", StringComparison.OrdinalIgnoreCase) || givenName.Equals("sys_admin", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }

    [PermissionAuthorize("FrontalUserManagement.Read")]
    [HttpGet("frontal")]
    public async Task<IActionResult> GetAllFrontalUsers()
    {
        try
        {
            var isDeveloper = IsCurrentUserDeveloper();
            var isAdmin = IsCurrentUserAdmin();

            // استخدام IgnoreQueryFilters() لإرجاع جميع المستخدمين بما فيهم IsActive = false
            var query = _context.Set<User>()
                .IgnoreQueryFilters()
                .Where(u => u.IsFrontal == true)
                .AsQueryable();

            if (!isDeveloper)
            {
                var devRoleId = await _context.Set<Role>()
                    .Where(r => r.Name == "System.Developer")
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                var devUserIds = new List<Guid>();
                if (devRoleId != Guid.Empty)
                {
                    devUserIds = await _context.Set<UserRole>()
                        .Where(ur => ur.RoleId == devRoleId && ur.UserId.HasValue)
                        .Select(ur => ur.UserId.Value)
                        .ToListAsync();
                }

                query = query.Where(u => u.UserName != "sys_developer" 
                                      && u.UserName != "developer" 
                                      && u.Email != "developer@smartapro.com" 
                                      && !devUserIds.Contains(u.Id));

                if (!isAdmin)
                {
                    // إذا لم يكن مطوراً ولا أدمن (مستخدم عادي): لا يرى حساب الأدمن أيضاً
                    var adminRoleId = await _context.Set<Role>()
                        .Where(r => r.Name == "System.Admin")
                        .Select(r => r.Id)
                        .FirstOrDefaultAsync();

                    var adminUserIds = new List<Guid>();
                    if (adminRoleId != Guid.Empty)
                    {
                        adminUserIds = await _context.Set<UserRole>()
                            .Where(ur => ur.RoleId == adminRoleId && ur.UserId.HasValue)
                            .Select(ur => ur.UserId.Value)
                            .ToListAsync();
                    }

                    query = query.Where(u => u.UserName != "admin" 
                                          && u.Email != "admin@admin.com" 
                                          && !adminUserIds.Contains(u.Id));
                }
            }

            var users = await query.ToListAsync();
            var result = _mapper.Map<List<UserAccountDto>>(users);
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while getting all frontal users : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpPost("frontal/{id:guid}")]
    public async Task<IActionResult> MakeUserFrontal(Guid id)
    {
        try
        {
            var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id == id);
            if (userQuery.Data == null) return NotFound("User not found");
            var user = await userQuery.Data.FirstOrDefaultAsync();
            if (user == null) return NotFound("User not found");
            
            user.IsFrontal = true;
            
            var userRolesQuery = await _repositoriesFactory.GetRepository<UserRole>().Filter(ur => ur.UserId == id);
            var userRoles = await userRolesQuery.Data.ToListAsync();
            foreach (var userRole in userRoles)
            {
                await _repositoriesFactory.GetRepository<UserRole>().Delete(userRole);
            }
            
            var result = await _repositoriesFactory.GetRepository<User>().Update(user);
            if (result.ErrorCode == ErrorCodes.Success) 
            {
                var updatedUser = _mapper.Map<UserAccountDto>(result.Data);
                return Ok(updatedUser);
            }

            return StatusCode(500, "An error occurred while making user frontal.");
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while making user frontal : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [PermissionAuthorize("UserManagement.Read")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var isDeveloper = IsCurrentUserDeveloper();
            var isAdmin = IsCurrentUserAdmin();

            // استخدام IgnoreQueryFilters() لإرجاع جميع المستخدمين بما فيهم IsActive = false
            var query = _context.Set<User>()
                .IgnoreQueryFilters()
                .AsQueryable();

            if (!isDeveloper)
            {
                // إذا لم يكن مطوراً: لا يرى حساب المطور (sys_developer)
                var devRoleId = await _context.Set<Role>()
                    .Where(r => r.Name == "System.Developer")
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                var devUserIds = new List<Guid>();
                if (devRoleId != Guid.Empty)
                {
                    devUserIds = await _context.Set<UserRole>()
                        .Where(ur => ur.RoleId == devRoleId && ur.UserId.HasValue)
                        .Select(ur => ur.UserId.Value)
                        .ToListAsync();
                }

                query = query.Where(u => u.UserName != "sys_developer" 
                                      && u.UserName != "developer" 
                                      && u.Email != "developer@smartapro.com" 
                                      && !devUserIds.Contains(u.Id));

                if (!isAdmin)
                {
                    // إذا لم يكن مطوراً ولا أدمن (مستخدم عادي): لا يرى حساب الأدمن أيضاً
                    var adminRoleId = await _context.Set<Role>()
                        .Where(r => r.Name == "System.Admin")
                        .Select(r => r.Id)
                        .FirstOrDefaultAsync();

                    var adminUserIds = new List<Guid>();
                    if (adminRoleId != Guid.Empty)
                    {
                        adminUserIds = await _context.Set<UserRole>()
                            .Where(ur => ur.RoleId == adminRoleId && ur.UserId.HasValue)
                            .Select(ur => ur.UserId.Value)
                            .ToListAsync();
                    }

                    query = query.Where(u => u.UserName != "admin" 
                                          && u.Email != "admin@admin.com" 
                                          && !adminUserIds.Contains(u.Id));
                }
            }

            var users = await query.ToListAsync();
            var result = _mapper.Map<List<UserAccountDto>>(users);
            return Ok(result);
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while getting all users : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [PermissionAuthorize("UserManagement.Create")]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserAccountDto userAccountDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var user = _mapper.Map<User>(userAccountDto);
            var result = await _repositoriesFactory.GetRepository<User>().Add(user);
            if (result.ErrorCode == ErrorCodes.Success)
            {
                var created = _mapper.Map<UserAccountDto>(result.Data);
                return Ok(created);
            }
            return StatusCode(500, "An error occurred while creating user");
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, ex.Message);
        }
    }

    #region OldUpdate
    //[HttpPut]

    //public async Task<IActionResult> UpdateUser([FromBody] UserAccountDto userAccountDto)
    //{
    //    try
    //    {
    //        if (ModelState.IsValid)
    //        {

    //            var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id == userAccountDto.Id);
    //            if (userQuery.Data == null) return NotFound("User not found");
    //            var user = await userQuery.Data.FirstOrDefaultAsync();
    //            if (user == null) return NotFound("User not found");
    //            user.UserName = userAccountDto.UserName;
    //            user.FirstName = userAccountDto.FirstName;
    //            user.MiddleName = userAccountDto.MiddleName;
    //            user.LastName = userAccountDto.LastName;
    //            user.PhoneNumber = userAccountDto.PhoneNumber;
    //            user.WorkId = userAccountDto.WorkId;
    //            user.TwoFactorEnabled = userAccountDto.TwoFactorEnabled;

    //            var result = await _repositoriesFactory.GetRepository<User>().Update(user);
    //            if (result != null) return Ok();

    //            return StatusCode(500, "An error occurred while creating the OTP.");
    //        }

    //        return BadRequest();
    //    }
    //    catch (Exception ex)
    //    {
    //        LogService.LogError($"Error occurred while creating user : {ex}");
    //        return StatusCode(500, "An unexpected error occurred");
    //    }
    //}
    #endregion

    [PermissionAuthorize("UserManagement.Update")]
    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] UserAccountDto userAccountDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var isDeveloper = IsCurrentUserDeveloper();
            var isAdmin = IsCurrentUserAdmin();

            // استخدام IgnoreQueryFilters() للبحث عن المستخدم حتى لو كان IsActive = false
            var user = await _context.Set<User>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userAccountDto.Id);

            if (user == null)
                return NotFound("User not found");

            // حماية حساب المطور من التعديل بواسطة غير المطورين
            if (!isDeveloper && (user.UserName == "sys_developer" || user.UserName == "developer" || user.Email == "developer@smartapro.com"))
            {
                return StatusCode(403, "غير مصرح لك بتعديل بيانات حساب المطور");
            }

            // حماية حساب المدير العام من التعديل بواسطة المستخدمين العاديين
            if (!isDeveloper && !isAdmin && (user.UserName == "admin" || user.Email == "admin@admin.com"))
            {
                return StatusCode(403, "غير مصرح لك بتعديل بيانات حساب المدير العام");
            }

            var userRepo = _repositoriesFactory.GetRepository<User>();

            user.UserName = userAccountDto.UserName;
            user.FirstName = userAccountDto.FirstName;
            user.MiddleName = userAccountDto.MiddleName;
            user.LastName = userAccountDto.LastName;
            user.PhoneNumber = userAccountDto.PhoneNumber;
            user.WorkId = userAccountDto.WorkId;
            user.TwoFactorEnabled = userAccountDto.TwoFactorEnabled;
            user.Email = userAccountDto.Email;
            user.DateOfBirth = userAccountDto.DateOfBirth;
            user.IsActive = userAccountDto.IsActive;
            user.IsFrontal = userAccountDto.IsFrontal;

            var result = await userRepo.Update(user);
            if (result.ErrorCode == ErrorCodes.Success)
            {
                // إعادة تحميل المستخدم من قاعدة البيانات للحصول على أحدث البيانات
                var updatedUserFromDb = await _context.Set<User>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == userAccountDto.Id);
                
                if (updatedUserFromDb != null)
                {
                    var updatedUser = _mapper.Map<UserAccountDto>(updatedUserFromDb);
                    return Ok(updatedUser);
                }
            }

            return StatusCode(500, $"An error occurred while updating the user. ErrorCode: {result.ErrorCode}");
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while updating user: {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }


    [PermissionAuthorize("UserManagement.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var isDeveloper = IsCurrentUserDeveloper();

                var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id == id);
                if (userQuery.Data == null) return NotFound("User not found");
                var user = await userQuery.Data.FirstOrDefaultAsync();
                if (user == null) return NotFound("User not found");

                // حماية حساب المدير العام وحساب المطور من الحذف
                if (user.UserName == "admin" || user.Email == "admin@admin.com")
                {
                    return BadRequest("لا يمكن حذف حساب المدير العام للنظام");
                }

                if (user.UserName == "sys_developer" || user.UserName == "developer" || user.Email == "developer@smartapro.com")
                {
                    if (!isDeveloper)
                    {
                        return StatusCode(403, "غير مصرح لك بحذف حساب المطور");
                    }
                    return BadRequest("لا يمكن حذف حساب المطور الرئيسي");
                }

                user.IsActive = false;
                var result = await _repositoriesFactory.GetRepository<User>().Update(user);
                if (result.ErrorCode == ErrorCodes.Success) 
                {
                    return Ok();
                }

                return StatusCode(500, "An error occurred while deleting the user.");
            }

            return BadRequest();
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while deleting user : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [PermissionAuthorize("UserManagement.Update")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] SmartAndPro.ERP.Application.Common.Dtos.Auth.Authentication.ResetUserPasswordDto dto)
    {
        try
        {
            if (dto == null || dto.UserId == Guid.Empty)
                return BadRequest("بيانات المستخدم غير صالحة");

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                return BadRequest("كلمة المرور يجب ألا تقل عن 6 خانات");

            var isDeveloper = IsCurrentUserDeveloper();
            var isAdmin = IsCurrentUserAdmin();

            var user = await _context.Set<User>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null)
                return NotFound("المستخدم غير موجود");

            if (!isDeveloper && (user.UserName == "sys_developer" || user.UserName == "developer" || user.Email == "developer@smartapro.com"))
            {
                return StatusCode(403, "غير مصرح لك بإعادة تعيين كلمة مرور حساب المطور");
            }

            if (!isDeveloper && !isAdmin && (user.UserName == "admin" || user.Email == "admin@admin.com"))
            {
                return StatusCode(403, "غير مصرح لك بإعادة تعيين كلمة مرور حساب المدير العام");
            }

            user.Password = SmartAndPro.ERP.Application.Services.Auth.Security.SecurityService.ComputeHMAC(dto.NewPassword, "UserID");

            var result = await _repositoriesFactory.GetRepository<User>().Update(user);
            if (result.ErrorCode == ErrorCodes.Success)
            {
                return Ok(new { message = "تم تغيير كلمة المرور بنجاح" });
            }

            return StatusCode(500, "حدث خطأ أثناء تغيير كلمة المرور");
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while resetting password : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }
}
