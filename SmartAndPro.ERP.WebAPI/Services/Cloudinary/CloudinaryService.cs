using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SmartAndPro.ERP.WebAPI.Configurations;

namespace SmartAndPro.ERP.WebAPI.Services.Cloudinary;

public class CloudinaryService : ICloudinaryService
{
    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/jpg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const string DefaultTransformation = "q_auto:best,f_auto";
    private const int DefaultMaxWidth = 1200;
    private const int DefaultQuality = 85;
    private const int ThumbnailWidth = 300;
    private const int ThumbnailQuality = 80;

    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> cloudinarySettings)
    {
        var settings = cloudinarySettings.Value;
        if (string.IsNullOrWhiteSpace(settings.CloudName) ||
            string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            throw new InvalidOperationException("Cloudinary settings are not configured.");
        }

        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string folder, CancellationToken cancellationToken = default, CloudinaryTransformation? transformation = null)
    {
        ValidateFile(file);

        await using var stream = file.OpenReadStream();
        var uploadParams = BuildUploadParams(file.FileName, stream, folder, transformation);
        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error is not null)
        {
            throw new InvalidOperationException(uploadResult.Error.Message);
        }

        var thumbnailUrl = GetThumbnailUrl(uploadResult.PublicId);
        return new CloudinaryUploadResult(uploadResult.SecureUrl.ToString(), uploadResult.PublicId, thumbnailUrl);
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(byte[] fileBytes, string fileName, string contentType, string folder, CancellationToken cancellationToken = default, CloudinaryTransformation? transformation = null)
    {
        if (fileBytes is null || fileBytes.Length == 0)
        {
            throw new InvalidOperationException("File is required.");
        }

        if (fileBytes.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("File size must be 5 MB or less.");
        }

        var detectedContentType = DetectContentType(fileBytes);
        if (!AllowedImageTypes.Contains(detectedContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Unsupported file type: {detectedContentType}");
        }

        var ext = detectedContentType.Split('/')[^1];
        await using var stream = new MemoryStream(fileBytes, writable: false);
        var uploadParams = BuildUploadParams(fileName ?? $"migration_{Guid.NewGuid()}.{ext}", stream, folder, transformation);
        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error is not null)
        {
            throw new InvalidOperationException(uploadResult.Error.Message);
        }

        var thumbnailUrl = GetThumbnailUrl(uploadResult.PublicId);
        return new CloudinaryUploadResult(uploadResult.SecureUrl.ToString(), uploadResult.PublicId, thumbnailUrl);
    }

    public string GetThumbnailUrl(string publicId, int width = ThumbnailWidth, int quality = ThumbnailQuality)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return string.Empty;

        return _cloudinary.Api.UrlImgUp
            .Transform(new Transformation()
                .Width(width)
                .Quality(quality)
                .Crop("limit")
                .FetchFormat("auto"))
            .BuildUrl(publicId);
    }

    public string GetOptimizedUrl(string publicId, int width = DefaultMaxWidth, int quality = DefaultQuality)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return string.Empty;

        return _cloudinary.Api.UrlImgUp
            .Transform(new Transformation()
                .Width(width)
                .Quality(quality)
                .Crop("limit")
                .FetchFormat("auto"))
            .BuildUrl(publicId);
    }

    public async Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return;

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        if (result.Error is not null)
        {
            throw new InvalidOperationException($"Failed to delete image from Cloudinary: {result.Error.Message}");
        }
    }

    public async Task<int> CleanupTempUploadsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        var tempFolders = new[] { "publicHotel/main/temp", "publicHotel/additional/temp", "publicRoomType/images/temp" };
        var cutoff = DateTime.UtcNow - maxAge;
        var totalDeleted = 0;

        foreach (var folder in tempFolders)
        {
            try
            {
                var result = await _cloudinary.ListResourcesAsync(new ListResourcesByPrefixParams
                {
                    Prefix = folder,
                    Type = "upload",
                    MaxResults = 500
                });

                if (result.Resources is null) continue;

                foreach (var resource in result.Resources)
                {
                    if (DateTime.TryParse(resource.CreatedAt, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var createdAt) && createdAt < cutoff)
                    {
                        try
                        {
                            await _cloudinary.DestroyAsync(new DeletionParams(resource.PublicId));
                            totalDeleted++;
                        }
                        catch { /* fault-tolerant */ }
                    }
                }
            }
            catch { /* skip folders that don't exist */ }
        }

        return totalDeleted;
    }

    private ImageUploadParams BuildUploadParams(string fileName, Stream stream, string folder, CloudinaryTransformation? transformation)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = folder,
            Transformation = BuildTransformation(transformation)
        };

        return uploadParams;
    }

    private Transformation BuildTransformation(CloudinaryTransformation? t)
    {
        var transformation = new Transformation();

        var width = t?.Width ?? DefaultMaxWidth;
        var quality = t?.Quality ?? DefaultQuality;
        var crop = t?.Crop ?? "limit";

        transformation.Width(width).Quality(quality).Crop(crop);

        var format = t?.Format;
        if (!string.IsNullOrWhiteSpace(format))
        {
            transformation.FetchFormat(format);
        }

        return transformation;
    }

    public string? ExtractPublicIdFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        try
        {
            var uri = new Uri(url);
            var segments = uri.Segments;
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].TrimEnd('/') == "upload" && i + 2 < segments.Length)
                {
                    var versionSegment = segments[i + 1].TrimEnd('/');
                    if (versionSegment.StartsWith("v") && versionSegment.Length > 1 && long.TryParse(versionSegment[1..], out _))
                    {
                        if (i + 2 < segments.Length)
                        {
                            var fullPath = string.Concat(segments.Skip(i + 2));
                            var dotIndex = fullPath.LastIndexOf('.');
                            return dotIndex > 0 ? fullPath[..dotIndex] : fullPath;
                        }
                    }
                }
            }
        }
        catch { /* ignore parse errors */ }

        return null;
    }

    private static string DetectContentType(byte[] bytes)
    {
        if (bytes.Length >= 4)
        {
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return "image/png";

            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return "image/jpeg";

            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
                return "image/gif";

            if (bytes.Length >= 12 && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                return "image/webp";
        }

        throw new InvalidOperationException("Unrecognized image format.");
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("File is required.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("File size must be 5 MB or less.");
        }

        if (!AllowedImageTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }
    }
}
