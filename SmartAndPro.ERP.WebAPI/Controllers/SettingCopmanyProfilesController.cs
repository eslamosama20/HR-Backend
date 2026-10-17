using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.CompanyDetails;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Inventory.CompanyManagement;
using SmartAndPro.ERP.SharedKernel.Constants;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[PermissionAuthorize(Permissions.CompanyDetails.Read)]
public class SettingCopmanyProfilesController : ControllerBase
{
    private readonly ILogger<SettingCopmanyProfilesController> _logger;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly IMapper _mapper;

    public SettingCopmanyProfilesController(
        ILogger<SettingCopmanyProfilesController> logger,
        ServiceBaseFactory serviceBaseFactory,
        IMapper mapper)
    {
        _logger = logger;
        _serviceBaseFactory = serviceBaseFactory;
        _mapper = mapper;
    }

    [HttpGet("getCompany")]
    public async Task<IActionResult> getCompany()
    {
        var companyProfileQuery =
            await _serviceBaseFactory.GetService<CompanyService>().GetAllAsync(new CompanyDetailsFiltersDto());

        var companyProfile = companyProfileQuery.Data.FirstOrDefault();

        if (companyProfile == null) return NotFound();

        return Ok(companyProfile);
    }
}
