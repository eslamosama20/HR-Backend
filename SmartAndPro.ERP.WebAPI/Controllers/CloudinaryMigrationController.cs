using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Domains.Entities.Reservation;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Services.Cloudinary;
using System.Text.Json;

namespace SmartAndPro.ERP.WebAPI.Controllers;

[Route("api/cloudinary")]
[ApiController]
[Authorize]
public class CloudinaryMigrationController : ControllerBase
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly ICloudinaryService _cloudinaryService;

    public CloudinaryMigrationController(RepositoriesFactory repositoriesFactory, ICloudinaryService cloudinaryService)
    {
        _repositoriesFactory = repositoriesFactory;
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("migrate-base64")]
    public async Task<IActionResult> MigrateBase64(CancellationToken cancellationToken)
    {
        var summary = new
        {
            roomTypes = await MigrateRoomTypes(cancellationToken),
            reservationServices = await MigrateReservationServices(cancellationToken),
            hotels = await MigrateHotels(cancellationToken),
            companyDetails = await MigrateCompanyDetails(cancellationToken),
        };

        return Ok(summary);
    }

    [HttpPost("migrate-public-hotels")]
    public async Task<IActionResult> MigratePublicHotels([FromQuery] bool dryRun = false, CancellationToken cancellationToken = default)
    {
        var result = await MigratePublicHotelsAsync(dryRun, cancellationToken);
        return Ok(result);
    }

    [HttpPost("migrate-public-room-types")]
    public async Task<IActionResult> MigratePublicRoomTypes([FromQuery] bool dryRun = false, CancellationToken cancellationToken = default)
    {
        var result = await MigratePublicRoomTypesAsync(dryRun, cancellationToken);
        return Ok(result);
    }

    [HttpPost("migrate-public-booking")]
    public async Task<IActionResult> MigratePublicBooking([FromQuery] bool dryRun = false, CancellationToken cancellationToken = default)
    {
        var hotels = await MigratePublicHotelsAsync(dryRun, cancellationToken);
        var roomTypes = await MigratePublicRoomTypesAsync(dryRun, cancellationToken);

        return Ok(new { hotels, roomTypes });
    }

    private async Task<object> MigrateRoomTypes(CancellationToken cancellationToken)
    {
        var repo = _repositoriesFactory.GetRepository<RoomType>();
        var getAll = await repo.GetAll();
        var items = getAll.Data?.ToList() ?? [];

        var migratedLogos = 0;
        var migratedGalleries = 0;
        var clearedBase64Fields = 0;
        var skippedDueToError = 0;

        foreach (var item in items)
        {
            var changed = false;

            if (item.Logo is { Length: > 0 })
            {
                try
                {
                    var upload = await _cloudinaryService.UploadImageAsync(item.Logo, $"room-type-{item.Id}.png", "image/png", "roomType/logo", cancellationToken);
                    item.LogoUrl = upload.Url;
                    item.LogoPublicId = upload.PublicId;
                    item.Logo = null;
                    migratedLogos++;
                    clearedBase64Fields++;
                    changed = true;
                }
                catch (Exception ex)
                {
                    // TODO: Log($"Error migrating room type {item.Id} logo: {ex.Message}");
                    skippedDueToError++;
                }
            }

            if (!string.IsNullOrWhiteSpace(item.Images))
            {
                try
                {
                    var base64Images = JsonSerializer.Deserialize<List<string>>(item.Images) ?? [];
                    var migratedUrls = new List<string>();
                    var index = 0;

                    foreach (var base64 in base64Images.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        var bytes = Convert.FromBase64String(base64);
                        var upload = await _cloudinaryService.UploadImageAsync(bytes, $"room-type-{item.Id}-img-{index}.png", "image/png", "roomType/images", cancellationToken);
                        migratedUrls.Add(upload.Url);
                        index++;
                    }

                    if (migratedUrls.Count > 0)
                    {
                        item.ImageUrls = JsonSerializer.Serialize(migratedUrls);
                        item.Images = null;
                        migratedGalleries++;
                        clearedBase64Fields++;
                        changed = true;
                    }
                }
                catch
                {
                    // skip malformed legacy images payload
                }
            }

            if (changed)
            {
                await repo.Update(item);
            }
        }

        return new { total = items.Count, migratedLogos, migratedGalleries, clearedBase64Fields, skippedDueToError };
    }

    private async Task<object> MigrateReservationServices(CancellationToken cancellationToken)
    {
        var repo = _repositoriesFactory.GetRepository<ReservationService>();
        var getAll = await repo.GetAll();
        var items = getAll.Data?.ToList() ?? [];

        var migratedLogos = 0;
        var skippedDueToError = 0;
        foreach (var item in items)
        {
            if (item.Logo is not { Length: > 0 })
            {
                continue;
            }

            try
            {
                var upload = await _cloudinaryService.UploadImageAsync(item.Logo, $"reservation-service-{item.Id}.png", "image/png", "reservationService/logo", cancellationToken);
                item.LogoUrl = upload.Url;
                item.LogoPublicId = upload.PublicId;
                item.Logo = null;
                await repo.Update(item);
                migratedLogos++;
            }
            catch (Exception ex)
            {
                // TODO: Log($"Error migrating reservation service {item.Id} logo: {ex.Message}");
                skippedDueToError++;
            }
        }

        return new { total = items.Count, migratedLogos, clearedBase64Fields = migratedLogos, skippedDueToError };
    }

    private async Task<object> MigrateHotels(CancellationToken cancellationToken)
    {
        var repo = _repositoriesFactory.GetRepository<Hotel>();
        var getAll = await repo.GetAll();
        var items = getAll.Data?.ToList() ?? [];

        var migratedLogos = 0;
        var skippedDueToError = 0;
        foreach (var item in items)
        {
            if (item.Logo is not { Length: > 0 })
            {
                continue;
            }

            try
            {
                var upload = await _cloudinaryService.UploadImageAsync(item.Logo, $"hotel-{item.Id}.png", "image/png", "hotel/logo", cancellationToken);
                item.LogoUrl = upload.Url;
                item.LogoPublicId = upload.PublicId;
                item.Logo = null;
                await repo.Update(item);
                migratedLogos++;
            }
            catch (Exception ex)
            {
                // TODO: Log($"Error migrating hotel {item.Id} logo: {ex.Message}");
                skippedDueToError++;
            }
        }

        return new { total = items.Count, migratedLogos, clearedBase64Fields = migratedLogos, skippedDueToError };
    }

    private async Task<object> MigrateCompanyDetails(CancellationToken cancellationToken)
    {
        var repo = _repositoriesFactory.GetRepository<CompanyDetails>();
        var getAll = await repo.GetAll();
        var items = getAll.Data?.ToList() ?? [];

        var migratedLogos = 0;
        var migratedStamps = 0;
        var skippedDueToError = 0;

        foreach (var item in items)
        {
            var changed = false;

            if (item.Logo is { Length: > 0 })
            {
                try
                {
                    var upload = await _cloudinaryService.UploadImageAsync(item.Logo, $"company-logo-{item.Id}.png", "image/png", "company/logo", cancellationToken);
                    item.LogoUrl = upload.Url;
                    item.Logo = null;
                    migratedLogos++;
                    changed = true;
                }
                catch (Exception ex)
                {
                    // TODO: Log($"Error migrating company {item.Id} logo: {ex.Message}");
                    skippedDueToError++;
                }
            }

            if (item.Stamp is { Length: > 0 })
            {
                try
                {
                    var upload = await _cloudinaryService.UploadImageAsync(item.Stamp, $"company-stamp-{item.Id}.png", "image/png", "company/stamp", cancellationToken);
                    item.StampUrl = upload.Url;
                    item.Stamp = null;
                    migratedStamps++;
                    changed = true;
                }
                catch (Exception ex)
                {
                    // TODO: Log($"Error migrating company {item.Id} stamp: {ex.Message}");
                    skippedDueToError++;
                }
            }

            if (changed)
            {
                await repo.Update(item);
            }
        }

        return new { total = items.Count, migratedLogos, migratedStamps, clearedBase64Fields = migratedLogos + migratedStamps, skippedDueToError };
    }

    private static byte[]? DecodeBase64(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
            return null;

        var cleaned = base64;
        var commaIndex = base64.IndexOf(',');
        if (commaIndex >= 0)
            cleaned = base64[(commaIndex + 1)..];

        cleaned = cleaned.Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "");

        if (string.IsNullOrWhiteSpace(cleaned))
            return null;

        try
        {
            return Convert.FromBase64String(cleaned);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private async Task<object> MigratePublicHotelsAsync(bool dryRun, CancellationToken cancellationToken)
    {
        var repo = _repositoriesFactory.GetRepository<PublicHotel>();
        var getAll = await repo.GetAll();
        var items = getAll.Data?.ToList() ?? [];

        var migratedMainImages = 0;
        var migratedAdditionalImages = 0;
        var skippedDueToError = 0;
        var skippedNoData = 0;

        foreach (var item in items)
        {
            // Skip if already migrated (has URL but still has base64 data)
            if (string.IsNullOrWhiteSpace(item.MainImage) && string.IsNullOrWhiteSpace(item.AdditionalImages))
            {
                skippedNoData++;
                continue;
            }

            var changed = false;

            // Migrate main image
            if (!string.IsNullOrWhiteSpace(item.MainImage))
            {
                var bytes = DecodeBase64(item.MainImage);
                if (bytes is not null)
                {
                    if (dryRun)
                    {
                        migratedMainImages++;
                    }
                    else
                    {
                        try
                        {
                            var upload = await _cloudinaryService.UploadImageAsync(bytes, $"public-hotel-{item.Id}-main.png", "image/png", "publicHotel/main", cancellationToken);
                            item.MainImageUrl = upload.Url;
                            item.MainImagePublicId = upload.PublicId;
                            item.MainImage = null;
                            migratedMainImages++;
                            changed = true;
                        }
                        catch (Exception ex)
                        {
                            // TODO: Log($"Error migrating public hotel {item.Id} main image: {ex.Message}");
                            skippedDueToError++;
                        }
                    }
                }
                else
                {
                    // TODO: Log($"Skipping public hotel {item.Id}: malformed base64 main image");
                    skippedDueToError++;
                }
            }

            // Migrate additional images
            if (!string.IsNullOrWhiteSpace(item.AdditionalImages))
            {
                List<string> base64Images = [];

                // Try JSON array first, then fall back to comma-separated
                try
                {
                    base64Images = JsonSerializer.Deserialize<List<string>>(item.AdditionalImages) ?? [];
                }
                catch (JsonException)
                {
                    base64Images = item.AdditionalImages
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList();
                }

                base64Images = base64Images.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                if (base64Images.Count > 0)
                {
                    if (dryRun)
                    {
                        migratedAdditionalImages += base64Images.Count;
                    }
                    else
                    {
                        var migratedUrls = new List<string>();
                        var remainingBase64 = new List<string>();

                        for (var index = 0; index < base64Images.Count; index++)
                        {
                            var bytes = DecodeBase64(base64Images[index]);
                            if (bytes is null)
                            {
                                skippedDueToError++;
                                continue;
                            }

                            try
                            {
                                var upload = await _cloudinaryService.UploadImageAsync(bytes, $"public-hotel-{item.Id}-img-{index}", null, "publicHotel/additional", cancellationToken);
                                migratedUrls.Add(upload.Url);
                            }
                            catch (Exception ex)
                            {
                                skippedDueToError++;
                                remainingBase64.Add(base64Images[index]);
                            }
                        }

                        if (remainingBase64.Count > 0)
                        {
                            item.AdditionalImages = JsonSerializer.Serialize(remainingBase64);
                        }
                        else
                        {
                            item.AdditionalImages = null;
                        }

                        if (migratedUrls.Count > 0)
                        {
                            List<string> existingUrls = [];
                            if (!string.IsNullOrWhiteSpace(item.AdditionalImageUrls))
                            {
                                try { existingUrls = JsonSerializer.Deserialize<List<string>>(item.AdditionalImageUrls) ?? []; }
                                catch { /* ignore parse errors */ }
                            }

                            existingUrls.AddRange(migratedUrls);
                            item.AdditionalImageUrls = JsonSerializer.Serialize(existingUrls);
                            migratedAdditionalImages += migratedUrls.Count;
                            changed = true;
                        }
                    }
                }
            }

            if (!dryRun && changed)
            {
                await repo.Update(item);
            }
        }

        return new
        {
            total = items.Count,
            migratedMainImages,
            migratedAdditionalImages,
            clearedBase64Fields = migratedMainImages + (migratedAdditionalImages > 0 ? 1 : 0),
            skippedNoData,
            skippedDueToError
        };
    }

    private async Task<object> MigratePublicRoomTypesAsync(bool dryRun, CancellationToken cancellationToken)
    {
        var repo = _repositoriesFactory.GetRepository<PublicRoomType>();
        var getAll = await repo.GetAll();
        var items = getAll.Data?.ToList() ?? [];

        var migratedMainImages = 0;
        var migratedAdditionalImages = 0;
        var skippedDueToError = 0;
        var skippedNoData = 0;

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.MainImage) && string.IsNullOrWhiteSpace(item.AdditionalImages))
            {
                skippedNoData++;
                continue;
            }

            var changed = false;

            // Migrate main image
            if (!string.IsNullOrWhiteSpace(item.MainImage))
            {
                var bytes = DecodeBase64(item.MainImage);
                if (bytes is not null)
                {
                    if (dryRun)
                    {
                        migratedMainImages++;
                    }
                    else
                    {
                        try
                        {
                            var upload = await _cloudinaryService.UploadImageAsync(bytes, $"public-room-type-{item.Id}-main.png", "image/png", "publicRoomType/images", cancellationToken);
                            item.MainImageUrl = upload.Url;
                            item.MainImagePublicId = upload.PublicId;
                            item.MainImage = null;
                            migratedMainImages++;
                            changed = true;
                        }
                        catch (Exception ex)
                        {
                            // TODO: Log($"Error migrating public room type {item.Id} main image: {ex.Message}");
                            skippedDueToError++;
                        }
                    }
                }
                else
                {
                    // TODO: Log($"Skipping public room type {item.Id}: malformed base64 main image");
                    skippedDueToError++;
                }
            }

            // Migrate additional images (JSON array of base64)
            if (!string.IsNullOrWhiteSpace(item.AdditionalImages))
            {
                List<string> base64Images = [];

                try
                {
                    base64Images = JsonSerializer.Deserialize<List<string>>(item.AdditionalImages) ?? [];
                }
                catch (JsonException)
                {
                    // not a JSON array — skip malformed
                    // TODO: Log($"Skipping public room type {item.Id} additional images: malformed JSON");
                    skippedDueToError++;
                    base64Images = [];
                }

                base64Images = base64Images.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                if (base64Images.Count > 0)
                {
                    if (dryRun)
                    {
                        migratedAdditionalImages += base64Images.Count;
                    }
                    else
                    {
                        var migratedUrls = new List<string>();
                        var remainingBase64 = new List<string>();

                        for (var index = 0; index < base64Images.Count; index++)
                        {
                            var bytes = DecodeBase64(base64Images[index]);
                            if (bytes is null)
                            {
                                skippedDueToError++;
                                continue;
                            }

                            try
                            {
                                var upload = await _cloudinaryService.UploadImageAsync(bytes, $"public-room-type-{item.Id}-img-{index}", null, "publicRoomType/images", cancellationToken);
                                migratedUrls.Add(upload.Url);
                            }
                            catch (Exception ex)
                            {
                                skippedDueToError++;
                                remainingBase64.Add(base64Images[index]);
                            }
                        }

                        if (remainingBase64.Count > 0)
                        {
                            item.AdditionalImages = JsonSerializer.Serialize(remainingBase64);
                        }
                        else
                        {
                            item.AdditionalImages = null;
                        }

                        if (migratedUrls.Count > 0)
                        {
                            List<string> existingUrls = [];
                            if (!string.IsNullOrWhiteSpace(item.AdditionalImageUrls))
                            {
                                try { existingUrls = JsonSerializer.Deserialize<List<string>>(item.AdditionalImageUrls) ?? []; }
                                catch { /* ignore parse errors */ }
                            }

                            existingUrls.AddRange(migratedUrls);
                            item.AdditionalImageUrls = JsonSerializer.Serialize(existingUrls);
                            migratedAdditionalImages += migratedUrls.Count;
                            changed = true;
                        }
                    }
                }
            }

            if (!dryRun && changed)
            {
                await repo.Update(item);
            }
        }

        return new
        {
            total = items.Count,
            migratedMainImages,
            migratedAdditionalImages,
            clearedBase64Fields = migratedMainImages + (migratedAdditionalImages > 0 ? 1 : 0),
            skippedNoData,
            skippedDueToError
        };
    }
}
