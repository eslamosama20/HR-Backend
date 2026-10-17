using Microsoft.AspNetCore.Http;

namespace SmartAndPro.ERP.WebAPI.Services.Cloudinary;

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default, CloudinaryTransformation? transformation = null);
    Task<CloudinaryUploadResult> UploadImageAsync(byte[] fileBytes, string fileName, string contentType, string folder, CancellationToken cancellationToken = default, CloudinaryTransformation? transformation = null);
    Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
    string GetThumbnailUrl(string publicId, int width = 300, int quality = 80);
    string GetOptimizedUrl(string publicId, int width = 1200, int quality = 85);
    Task<int> CleanupTempUploadsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
    string? ExtractPublicIdFromUrl(string url);
}

public record CloudinaryUploadResult(string Url, string PublicId, string? ThumbnailUrl = null);

public record CloudinaryTransformation(int? Width = null, int? Height = null, int? Quality = null, string? Format = null, string? Crop = null);
