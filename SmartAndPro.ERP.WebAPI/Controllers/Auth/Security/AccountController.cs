

using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Auth.Authorization;
using SmartAndPro.ERP.Application.Services.Auth;
using SmartAndPro.ERP.Application.Services.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Auth.Authorization;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Infrastructure.External;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Auth.Security;

[Route("api/auth/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly ServicesFactory _serviceFactory;
    private readonly TokenSettings _token;
    private readonly SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService _quotaService;


    public AccountController(ServicesFactory serviceFactory,
                             RepositoriesFactory repositoriesFactory,
                             IMapper mapper,
                             AppSettings appSettings,
                             SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService quotaService)
    {
        _token = appSettings.Token;
        _serviceFactory = serviceFactory;
        this._repositoriesFactory = repositoriesFactory;
        this._mapper = mapper;
        this._quotaService = quotaService;
    }

    
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDto)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _quotaService.ValidateUserQuotaAsync(User);
                var userModel = _mapper.Map<RegisterUserDTO, User>(registerUserDto);
                var userExistsQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.UserName == userModel.UserName || u.Email == userModel.Email);
                var userExists = await userExistsQuery.Data.AnyAsync();
                if (userExists)
                    return BadRequest("User with this email or username already exists");
                if (registerUserDto.UserRoles.Any())
                {
                    userModel.UserRoles = registerUserDto.UserRoles.Select(ur => new UserRole
                    {
                        RoleId = ur.RoleId
                    }).ToList();
                }
                var userAddQuery = await _repositoriesFactory.GetRepository<User>().Add(userModel);
                if (userAddQuery.Data is null ) 
                    return BadRequest(userAddQuery);
                
                var createdUserQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.UserName == userModel.UserName);
                if (createdUserQuery.Data == null)
                    return StatusCode(500, "A problem happened while creating the user.");
                var createdUser = await createdUserQuery.Data.FirstOrDefaultAsync();
                if (createdUser == null)
                    return StatusCode(500, "A problem happened while creating the user.");

                var otp = new OTPs
                {
                    OTPReferenceKey = createdUser.Id.ToString(),
                    ExpiryDate = DateTime.UtcNow.AddHours(1),
                    NumberOfFailedTrails = 0
                };
                var otpStatusQuery = await _repositoriesFactory.GetRepository<OTPStatuses>()
                    .Filter(s => s.EnglishDescription.ToLower() == "pending");
                var otpStatus = await otpStatusQuery.Data.FirstOrDefaultAsync();
                otp.StatusID = otpStatus?.StatusID;
                
                var otpTypeQuery = await _repositoriesFactory.GetRepository<OTPType>()
                    .Filter(t => t.Name.ToLower() == "sign up");
                var otpType = await otpTypeQuery.Data.FirstOrDefaultAsync();
                otp.OtpTypeID = otpType?.ID;
                
                var otpValue = OtpService.GenerateOtp();
                otp.OTPPayload = SecurityService.ComputeHMAC(otpValue, otp.OTPReferenceKey);
                try
                {
                    await EmailService.SendMail(userModel.Email, "Your One-Time Password for SmartAndPro", otpValue);
                }catch (TimeoutException e)
                {
                    LogService.LogError($"Error occurred while sending email : {e}");
                }
                
                var addOtpResultQuery = await _repositoriesFactory.GetRepository<OTPs>().Add(otp);
                var addOtpResult = addOtpResultQuery.Data;
                if (addOtpResult is  null)
                    return StatusCode(500, "An error occurred while creating the OTP.");

                return Ok(new { userId = userModel.Id, otpId = otp.OTPReference });
            }

            return BadRequest();
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while creating user : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpPost("Login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.UserName == loginDto.UserName);
                if (userQuery.Data == null) return NotFound("User not found");
                var user = await userQuery.Data.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);
                if (user == null) return NotFound("User not found");

                if (!user.IsActive)
                {
                    return BadRequest(new { message = "ACCOUNT_DEACTIVATED", error = "ACCOUNT_DEACTIVATED" });
                }

                var loginActionLogs = new LoginActionLogs();
                loginActionLogs.UserId = user.Id;
                loginActionLogs.LoginTime = DateTime.UtcNow;

                if (user.Password != SecurityService.ComputeHMAC(loginDto.Password, "UserID"))
                {
                    loginActionLogs.IsFailedTrail = true;
                    await _repositoriesFactory.GetRepository<LoginActionLogs>().Add(loginActionLogs);
                    return Unauthorized();
                }

                if (user.TwoFactorEnabled || !user.EmailVerified)
                {
                    var otpNumber = OtpService.GenerateOtp();
                    try
                    {
                        await EmailService.SendMail(user.Email, "Your One-Time Password for SmartAndPro", otpNumber);
                    }
                    catch (TimeoutException e)
                    {
                        LogService.LogError($"Error occurred while sending email : {e}");
                    }

                    var otpStatusIdQuery = await _repositoriesFactory.GetRepository<OTPStatuses>().Filter(s => s.EnglishDescription.ToLower() == "pending");
                    var otpStatus = await otpStatusIdQuery.Data.FirstOrDefaultAsync();
                    var otpTypeQuery = await _repositoriesFactory.GetRepository<OTPType>().Filter(t => t.Name.ToLower() == "sign in");
                    var otpType = await otpTypeQuery.Data.FirstOrDefaultAsync();
                    
                    var otp = new OTPs
                    {
                        OTPReferenceKey = user.Id.ToString(),
                        StatusID = otpStatus?.StatusID,
                        OtpTypeID = otpType?.ID,
                        ExpiryDate = DateTime.UtcNow.AddHours(1)
                    };

                    otp.OTPPayload = SecurityService.ComputeHMAC(otpNumber, otp.OTPReferenceKey);

                    var resultQuery = await _repositoriesFactory.GetRepository<OTPs>().Add(otp);
                    var result = resultQuery.Data;
                    if (result == null)
                        return StatusCode(500, "An error occurred while creating the OTP.");

                    return Ok(new { userId = user.Id, otpId = otp.OTPReference });
                }else 
                {
                    var token = await _serviceFactory.GetService<TokenService>().CreateToken(user);
                    var userDto = new UserDTO
                    {
                        authToken = token,
                        refreshToken = token, // Using same token as refresh for now
                        expiresIn = DateTime.UtcNow.AddDays(int.Parse(_token.DurationInDays))
                    };
                    
                    loginActionLogs.IsFailedTrail = false;
                    await _repositoriesFactory.GetRepository<LoginActionLogs>().Add(loginActionLogs);
                    
                    return Ok(userDto);
                }
            }

            return BadRequest();
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while Login : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Email == forgotPasswordDto.Email);
                if (userQuery.Data == null) return NotFound("User not found");
                var user = await userQuery.Data.FirstOrDefaultAsync();
                if (user == null) return NotFound("User not found");
                
                var otp = new OTPs
                {
                    OTPReferenceKey = user.Id.ToString(),
                    ExpiryDate = DateTime.UtcNow.AddHours(1),
                    NumberOfFailedTrails = 0
                };
                var otpStatusQuery = await _repositoriesFactory.GetRepository<OTPStatuses>()
                    .Filter(s => s.EnglishDescription.ToLower() == "pending");
                var otpStatus = await otpStatusQuery.Data.FirstOrDefaultAsync();
                otp.StatusID = otpStatus?.StatusID;
                
                var otpTypeQuery = await _repositoriesFactory.GetRepository<OTPType>()
                    .Filter(t => t.Name.ToLower() == "reset password");
                var otpType = await otpTypeQuery.Data.FirstOrDefaultAsync();
                otp.OtpTypeID = otpType?.ID;
                
                var otpValue = OtpService.GenerateOtp();
                otp.OTPPayload = SecurityService.ComputeHMAC(otpValue, otp.OTPReferenceKey);
                try
                {
                    await EmailService.SendMail(user.Email, "Reset your password One-Time Password for SmartAndPro", otpValue);
                }catch (TimeoutException e)
                {
                    LogService.LogError($"Error occurred while sending email : {e}");
                }
                var addOtpResultQuery = await _repositoriesFactory.GetRepository<OTPs>().Add(otp);
                var addOtpResult = addOtpResultQuery.Data;
                if (addOtpResult is  null)
                    return StatusCode(500, "An error occurred while creating the OTP.");
                return Ok(new { userId = user.Id, otpId = otp.OTPReference });
            }
            return BadRequest();
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while Forgot Password : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpPost("ResendOTP")]
    public async Task<IActionResult> ResendOTP(string userId)
    {
        try
        {
            var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id.ToString() == userId);
            if (userQuery.Data == null)
                return NotFound("User not found");
            var user = await userQuery.Data.FirstOrDefaultAsync();
            if (user == null)
                return NotFound("User not found");

            var otpNumber = OtpService.GenerateOtp();
            try{
                await EmailService.SendMail(user.Email, "Your One-Time Password for SmartAndPro", otpNumber);

            }
            catch (TimeoutException e)
            {
                LogService.LogError($"Error occurred while sending email : {e}");
            }
            
            var otpStatusQuery = await _repositoriesFactory.GetRepository<OTPStatuses>().Filter(s => s.EnglishDescription.ToLower() == "pending");
            var otpStatus = await otpStatusQuery.Data.FirstOrDefaultAsync();
            var otpTypeQuery = await _repositoriesFactory.GetRepository<OTPType>().Filter(t => t.Name.ToLower() == "sign in");
            var otpType = await otpTypeQuery.Data.FirstOrDefaultAsync();

            var otp = new OTPs
            {
                OTPReferenceKey = userId,
                StatusID = otpStatus?.StatusID,
                OtpTypeID = otpType?.ID,
                
                ExpiryDate = DateTime.UtcNow.AddHours(1),
                OTPPayload = SecurityService.ComputeHMAC(otpNumber, user.Id.ToString())
            };

            var resultQuery = await _repositoriesFactory.GetRepository<OTPs>().Add(otp);
            var result = resultQuery.Data;
            if (result == null)
                return StatusCode(500, "An error occurred while creating the OTP.");

            return Ok(otp.OTPReference);
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while resending OTP: {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpPost("VerifyOTPAndSetNewPassword")]
    public async Task<IActionResult> VerifyOTPAndSetNewPassword(VerifyOtpAndSetNewPasswordDTO verifyOtpAndSetNewPasswordDTO)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var otpQuery = await _repositoriesFactory.GetRepository<OTPs>()
                    .Filter(o => o.OTPReference.ToString() == verifyOtpAndSetNewPasswordDTO.OtpReference);
                var otp = await otpQuery.Data.FirstOrDefaultAsync();
                if (otp == null || otp.ExpiryDate <= DateTime.UtcNow || otp.NumberOfFailedTrails >= 3)
                    return BadRequest("Invalid or expired OTP.");

                var hashedOtp = SecurityService.ComputeHMAC(verifyOtpAndSetNewPasswordDTO.OtpValue, otp.OTPReferenceKey);
                if (hashedOtp != otp.OTPPayload)
                {
                    otp.NumberOfFailedTrails += 1;
                    if (otp.NumberOfFailedTrails >= 3)
                    {
                        var otpStatusQuery = await _repositoriesFactory.GetRepository<OTPStatuses>()
                            .Filter(o => o.EnglishDescription.ToLower() == "blocked");
                        var otpStatus = await otpStatusQuery.Data.FirstOrDefaultAsync();
                        otp.StatusID = otpStatus?.StatusID;
                    }

                    await _repositoriesFactory.GetRepository<OTPs>().Update(otp);
                    return BadRequest($"Invalid OTP, remainingAttempts : {3 - otp.NumberOfFailedTrails}");
                }

                {
                    var otpStatusQuery = await _repositoriesFactory.GetRepository<OTPStatuses>()
                        .Filter(o => o.EnglishDescription.ToLower() == "success");
                    var otpStatus = await otpStatusQuery.Data.FirstOrDefaultAsync();
                    otp.StatusID = otpStatus?.StatusID;
                    await _repositoriesFactory.GetRepository<OTPs>().Update(otp);
                    var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id.ToString() == verifyOtpAndSetNewPasswordDTO.UserId);
                    if (userQuery.Data == null) return NotFound("User not found");
                    var user = await userQuery.Data.FirstOrDefaultAsync();
                    if (user == null) return NotFound("User not found");
                    
                    
                    user.Password = SecurityService.ComputeHMAC(verifyOtpAndSetNewPasswordDTO.NewPassword, "UserID");
                    if (!user.EmailVerified)
                    {
                        user.EmailVerified = true;
                    }
                    var updateUserResultQuery = await _repositoriesFactory.GetRepository<User>().Update(user);
                    var updateUserResult = updateUserResultQuery.Data;
                    var userDto = new UserDTO();
                    if (updateUserResult != null)
                    {
                        userDto.authToken = await _serviceFactory.GetService<TokenService>().CreateToken(updateUserResult);
                        userDto.expiresIn = DateTime.UtcNow.AddDays(int.Parse(_token.DurationInDays));

                        return Ok(userDto);
                    }
                }
            }
            return BadRequest();
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while Verify OTP : {ex}");
            return StatusCode(500, ex.Message);
        }
    }

    [Authorize]
    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var userQuery = await _repositoriesFactory.GetRepository<User>()
                .Filter(u => u.Id.ToString() == userId);

            if (userQuery.Data == null)
            {
                return NotFound("User Not Found");
            }
            var user = await userQuery.Data.FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("User Not Found");
            }
            if (changePasswordDto.NewPassword == changePasswordDto.OldPassword)
            {
                return BadRequest("New password cannot be the same as the old password");
            }
            var oldPasswordHash = SecurityService.ComputeHMAC(changePasswordDto.OldPassword, "UserID");
            if(user.Password != oldPasswordHash)
            {
                return BadRequest("Old password is incorrect");
            }
            user.Password = SecurityService.ComputeHMAC(changePasswordDto.NewPassword, "UserID");

            var updateUserResultQuery = await _repositoriesFactory.GetRepository<User>()
                .Update(user);

            var updateUserResult = updateUserResultQuery.Data;

            var userDto = new UserDTO();

            if(updateUserResult != null)
            {
                userDto.authToken = await _serviceFactory.GetService<TokenService>()
                    .CreateToken(updateUserResult);

                userDto.expiresIn = DateTime.UtcNow.AddDays(int.Parse(_token.DurationInDays));

                return Ok(userDto);

            }      return BadRequest();

        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while ChangePassword : {ex}");
            return StatusCode(500, ex.Message);
        }
    }
    [Authorize] 
    [HttpPatch("UpdateProfile")]
    public async Task<IActionResult> UpdateUserProfile(UpdateUserProfileDTO updateUserProfileDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userQuery = await _repositoriesFactory.GetRepository<User>()
                .Filter(u=>u.Id.ToString() == userId);
   
            var user = await userQuery.Data.FirstOrDefaultAsync();
            if(user == null)
                return NotFound("User not found");

            user.FirstName = updateUserProfileDto.FirstName ?? user.FirstName;
            user.MiddleName = updateUserProfileDto.MiddleName ?? user.MiddleName;
            user.LastName = updateUserProfileDto.LastName ?? user.LastName;
            user.PhoneNumber = updateUserProfileDto.PhoneNumber ?? user.PhoneNumber;
            user.DateOfBirth = updateUserProfileDto.DateOfBirth ?? user.DateOfBirth;

            if (!string.IsNullOrEmpty(updateUserProfileDto.ProfilePicture))
            {
                try
                {
                    user.ProfilePicture = Convert.FromBase64String(updateUserProfileDto.ProfilePicture);
                }
                catch (FormatException)
                {
                    return BadRequest(new { error = "Invalid profile picture format. Please provide a valid Base64 string." });
                }
            }
            var updateUserResultQuery = await _repositoriesFactory.GetRepository<User>()
                .Update(user);

            var updateUserResult = updateUserResultQuery.Data;

            if (updateUserResult != null) 
            {
                var userDto = _mapper.Map<UserAccountDto>(updateUserResult);
                return Ok(userDto);
            }
              

            return BadRequest(new { error = "Failed to update profile" });
        }
        catch(Exception ex)
        {
            LogService.LogError($"Error occurred while UpdateProfile : {ex}");
            return StatusCode(500, "An error occurred while updating profile");

        }
    }

    [HttpPost("VerifyOTP")]
    public async Task<IActionResult> VerifyOTP(VerifyOtpDTO verifyOtpDTO)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var otpQuery = await _repositoriesFactory.GetRepository<OTPs>()
                    .Filter(o => o.OTPReference.ToString() == verifyOtpDTO.OtpReference);
                var otp = await otpQuery.Data.FirstOrDefaultAsync();
                if (otp == null || otp.ExpiryDate <= DateTime.UtcNow || otp.NumberOfFailedTrails >= 3)
                    return BadRequest("Invalid or expired OTP.");

                var hashedOtp = SecurityService.ComputeHMAC(verifyOtpDTO.OtpValue, otp.OTPReferenceKey);
                if (hashedOtp != otp.OTPPayload)
                {
                    otp.NumberOfFailedTrails += 1;
                    if (otp.NumberOfFailedTrails >= 3)
                    {
                        var otpStatusQuery = await _repositoriesFactory.GetRepository<OTPStatuses>()
                            .Filter(o => o.EnglishDescription.ToLower() == "blocked");
                        var otpStatus = await otpStatusQuery.Data.FirstOrDefaultAsync();
                        otp.StatusID = otpStatus?.StatusID;
                    }

                    await _repositoriesFactory.GetRepository<OTPs>().Update(otp);
                    return BadRequest($"Invalid OTP, remainingAttempts : {3 - otp.NumberOfFailedTrails}");
                }

                {
                    var otpStatusQuery = await _repositoriesFactory.GetRepository<OTPStatuses>()
                        .Filter(o => o.EnglishDescription.ToLower() == "success");
                    var otpStatus = await otpStatusQuery.Data.FirstOrDefaultAsync();
                    otp.StatusID = otpStatus?.StatusID;
                    await _repositoriesFactory.GetRepository<OTPs>().Update(otp);
                    var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id.ToString() == verifyOtpDTO.UserId);
                    if (userQuery.Data == null) return NotFound("User not found");
                    var user = await userQuery.Data.FirstOrDefaultAsync();
                    if (user == null) return NotFound("User not found");
                    if (!user.EmailVerified)
                    {
                        user.EmailVerified = true;
                        var updateUserResultQuery = await _repositoriesFactory.GetRepository<User>().Update(user);
                        var updateUserResult = updateUserResultQuery.Data;
                        var userDto = new UserDTO();
                        if (updateUserResult != null)
                        {
                            userDto.authToken = await _serviceFactory.GetService<TokenService>().CreateToken(updateUserResult);
                            userDto.expiresIn = DateTime.UtcNow.AddDays(int.Parse(_token.DurationInDays));

                            return Ok(userDto);
                        }
                    }
                    else
                    {
                        var userDto = new UserDTO();
                        userDto.authToken = await _serviceFactory.GetService<TokenService>().CreateToken(user);
                        userDto.expiresIn = DateTime.UtcNow.AddDays(int.Parse(_token.DurationInDays));
                        return Ok(userDto);
                    }
                    
                }
            }

            return BadRequest();
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while Verify Token : {ex}");
            return StatusCode(500, ex.Message);
        }
    }

    /*[HttpPost("AssignUserToCompany/{userId}/{companyId:guid}")]
    public async Task<IActionResult> AssignUserToCompany(string userId, Guid companyId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid)) return BadRequest("Invalid user ID format");
            var user = await repositoriesFactory.GetRepository<User>().GetAll().FirstOrDefaultAsync(u => u.Id.ToString() == userGuid.ToString());// userManager.FindByIdAsync(userGuid.ToString());
            var company = await repositoriesFactory.GetRepository<CompanyDetails>().GetAll()
                .FirstOrDefaultAsync(c => c.Id == companyId);
            if (user == null || company == null)
                return BadRequest(
                    $"There is no {(user == null ? "User" : "Company")} with ID : {(user == null ? userId : companyId)}");
            user.CompanyDetailsId = companyId;
            var result = await repositoriesFactory.GetRepository<User>().Update(user); //userManager.UpdateAsync(user);
            if (result != null) return Ok("User assigned successfully");
            var errors = result;
            return BadRequest("Failed to assign user: " + string.Join(", ", errors));
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while assign user to company : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }
    */

    
    [HttpDelete("RemoveUser/{userEmail}")]
    public async Task<IActionResult> RemoveUser(string userEmail)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Email == userEmail);
                if (userQuery.Data == null) return NotFound("User not found");
                var user = await userQuery.Data.FirstOrDefaultAsync();
                if (user == null) return NotFound("User not found");

                var result = await _repositoriesFactory.GetRepository<User>().Update(user);  // await userManager.UpdateAsync(user);
                // ?) Should it be == 
                if (result != null) return Ok();

                return StatusCode(500, "An error occurred while creating the OTP.");
            }

            return BadRequest();
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while creating user : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUserByToken()
    {
        try
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.Name);
            if (userId == null) return Unauthorized();
            var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id.ToString() == userId.Value);
            if (userQuery.Data == null) return NotFound(); // User not found
            var user = await userQuery.Data.FirstOrDefaultAsync();

            if (user == null) return NotFound(); // User not found
            var userAccount = _mapper.Map<UserAccountDto>(user);
            return Ok(userAccount);
        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while get user by token : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [Authorize]
    [HttpPost("toggle-two-factor-enabled")]
    public async Task<IActionResult> ToggleTwoFactorEnabled([FromBody] ToggleTwoFactorEnabledDto toggleTwoFactorEnabledDto)
    {
        try
        {
            var tokenUserId = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if (tokenUserId != toggleTwoFactorEnabledDto.UserId.ToString())
                return BadRequest("You can not update other user's data");
            
            var userQuery = await _repositoriesFactory.GetRepository<User>().Filter(u => u.Id == toggleTwoFactorEnabledDto.UserId);
            if (userQuery.Data == null) return NotFound("User not found");
            var user = await userQuery.Data.FirstOrDefaultAsync();
            if (user == null) return NotFound("User not found");
            user.TwoFactorEnabled = !user.TwoFactorEnabled;
            var result = await _repositoriesFactory.GetRepository<User>().Update(user);
            if (result != null) return Ok();

            return StatusCode(500, "An error occurred while creating the OTP.");

        }
        catch (Exception ex)
        {
            LogService.LogError($"Error occurred while updating user : {ex}");
            return StatusCode(500, "An unexpected error occurred");
        }
    }
}