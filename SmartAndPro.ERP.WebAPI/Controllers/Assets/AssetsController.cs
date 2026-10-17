using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;

namespace SmartAndPro.ERP.WebAPI.Controllers.Assets;

[Route("api/[controller]")]
[ApiController]
public class AssetsController : ControllerBase
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly ICloudinaryService _cloudinaryService;

    public AssetsController(RepositoriesFactory repositoriesFactory, ICloudinaryService cloudinaryService)
    {
        _repositoriesFactory = repositoriesFactory;
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] Guid? assetId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        try
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, "assets", cancellationToken);
            var base64 = await ReadFileAsBase64Async(file, cancellationToken);
            var assetRepo = _repositoriesFactory.GetRepository<Asset>();

            Asset? asset = null;
            if (assetId.HasValue)
            {
                var assetQuery = await assetRepo.Filter(x => x.Id == assetId.Value);
                asset = assetQuery.Data?.FirstOrDefault();
            }

            if (asset is null)
            {
                asset = new Asset
                {
                    URL = uploadResult.Url,
                    Base64Url = base64,
                    PublicId = uploadResult.PublicId
                };
                await assetRepo.Add(asset);
            }
            else
            {
                asset.URL = uploadResult.Url;
                asset.Base64Url = base64;
                asset.PublicId = uploadResult.PublicId;
                await assetRepo.Update(asset);
            }

            return Ok(new { id = asset.Id, base64Url = asset.Base64Url, url = asset.URL, publicId = asset.PublicId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<string> ReadFileAsBase64Async(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        await using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        return Convert.ToBase64String(ms.ToArray());
    }
}
