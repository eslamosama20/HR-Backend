using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnits;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.@base;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.Application.Services.Inventory.ProductManagement;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.SharedKernel.Utilities;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Utils;
using Microsoft.AspNetCore.Authorization;

namespace SmartAndPro.ERP.WebAPI.Controllers.Inventory.ProductManagement;

[Route("api/inventory/[controller]/")]
[ApiController]
[Authorize]
[PermissionAuthorize("InventoryManagement.Read")]
public class ProductUnitController : Controller
{
    private readonly ProductUnitService _service;
    private readonly ServiceBaseFactory _serviceBaseFactory;

    public ProductUnitController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<ProductUnitService>();
    }


    // ProductUnites get all

    [HttpGet("")]
    
    public async Task<IActionResult> GetProductUnites( [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null, [FromQuery] Guid? productDetailsId = null)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(new ProductUnitFiltersDto());


            result = responseDate.HandleResult(true);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // ProductUnites get by id
    [HttpGet("{id:guid}")]
    
    public async Task<IActionResult> GetProductUnitesById(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // ProductUnites create
    [HttpPost("")]
    
    public async Task<IActionResult> CreateProductUnites([FromBody] ProductUnitPostDto productUnites)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            if (productUnites.Quantity is not null && productUnites.Quantity > 1)
            {
                var responseDate = await _service.AddAsync(productUnites);
                result = responseDate.HandleResult();
            }
            else
            {
                var responseDate = await _service.AddAsync(productUnites);
                result = responseDate.HandleResult();
            }
            
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // ProductUnites update
    [HttpPut]
    
    public async Task<IActionResult> UpdateProductUnites([FromBody] ProductUnitPutDto productUnites)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(productUnites);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

    // ProductUnites delete
    [HttpDelete("{id:guid}")]
    
    public async Task<IActionResult> DeleteProductUnites(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var service = _serviceBaseFactory.GetService<ProductUnitService>();
            var responseDate = await service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }
}