using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.GeneralBankAccount;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Treasuries;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.GeneralBankAccounts;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;
using Zatca.EInvoice.SDK.Contracts.Models;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.GeneralBankAccounts
{
    [Route("api/[controller]")]
    [ApiController]
[Authorize]
[PermissionAuthorize("Account.Read")]
    public class GeneralBankAccountController : ControllerBase
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly GeneralBankAccountService _generalBankAccountService;
        private readonly SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService _quotaService;

        public GeneralBankAccountController(ServiceBaseFactory serviceBaseFactory, SmartAndPro.ERP.Application.Services.Auth.Interfaces.IRoleQuotaService quotaService)
        {
            _serviceBaseFactory = serviceBaseFactory;
            _generalBankAccountService = _serviceBaseFactory.GetService<GeneralBankAccountService>();
            _quotaService = quotaService;
        }

        [HttpGet("frontal")]
        public async Task<IActionResult> GetAllFrontal()
        {
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _generalBankAccountService.GetAllFrontalAsync();
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpPost("frontal/{id:guid}")]
        public async Task<IActionResult> MakeFrontalAsync(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _generalBankAccountService.MakeFrontalAsync(id);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetGeneralBankAccounts([FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            IActionResult result = NoContent();
            try
            {
                var responseDate = await _generalBankAccountService.GetAllAsync(new GeneralBankAccountFiltersDto(), pageNumber, pageSize);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetGeneralBankAccountById(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _generalBankAccountService.GetAsync(id);
                result = responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateGeneralBankAccount([FromBody] GeneralBankAccountPostDto generalBankAccountPostDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _quotaService.ValidateBankQuotaAsync(User);
                var responseDate = await _generalBankAccountService.AddAsync(generalBankAccountPostDto);
                return responseDate.HandleResult();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateGeneralBankAccount([FromBody] GeneralBankAccountPutDto generalBankAccountPutDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            IActionResult result = NoContent();

            try
            {
                var responseDate = await _generalBankAccountService.UpdateAsync(generalBankAccountPutDto);
                result = responseDate.HandleResult();
            }
            catch(Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteGeneralBankAccount(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            IActionResult result = NoContent();
            try
            {
                var responseDate = await _generalBankAccountService.DeleteAsync(id);
                result = responseDate.HandleResult();
            }catch(Exception ex)
            {
                LogService.LogError(ex);
            }
            return result;
            
        }

        [HttpPost("import")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided or file is empty." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return BadRequest(new { message = "Only .xlsx and .xls files are supported." });

            try
            {
                await using var stream = file.OpenReadStream();
                var importResult = await _generalBankAccountService.ImportFromExcelAsync(stream);
                return Ok(importResult);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return StatusCode(500, new { message = "Import failed unexpectedly.", error = ex.Message });
            }
        }

        [HttpGet("/api/generalBankAccounts/template")]
        [AllowAnonymous]
        public IActionResult DownloadTemplate()
        {
            var fileName = "bankAccount-template.xlsx";
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);

            if (!System.IO.File.Exists(templatePath))
                return NotFound(new { message = "Template file not found." });

            return PhysicalFile(templatePath, ExcelContentType, fileName);
        }
    }
}
