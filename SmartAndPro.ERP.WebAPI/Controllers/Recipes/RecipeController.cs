using Microsoft.AspNetCore.Authorization;
using SmartAndPro.ERP.WebAPI.Attributes;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Common.Dtos.Recipes.Recipe;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.Application.Services.Recipes;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Attributes;
using SmartAndPro.ERP.WebAPI.Utils;
using SmartAndPro.ERP.WebAPI.Attributes;

namespace SmartAndPro.ERP.WebAPI.Controllers.Recipes;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RecipeController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly RecipeService _service;
    public RecipeController(ServiceBaseFactory serviceBaseFactory)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<RecipeService>();
    }

    // Recipes get all
    [PermissionAuthorize("Recipe.Read")]
    [HttpGet("")]
    public async Task<IActionResult> GetRecipes([FromQuery] RecipeFiltersDto filters )
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(filters);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // Recipes get by id
    [PermissionAuthorize("Recipe.Read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRecipesById(Guid id)
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
    
    // Recipes create
    [PermissionAuthorize("Recipe.Create")]
    [HttpPost("")]
    public async Task<IActionResult> CreateRecipe([FromBody] RecipePostDto recipe)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(recipe);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // Recipes update
    [PermissionAuthorize("Recipe.Update")]
    [HttpPut]
    public async Task<IActionResult> UpdateRecipe([FromBody] RecipePutDto recipe)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(recipe);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // Recipes delete
    [PermissionAuthorize("Recipe.Delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRecipe(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

}