using SmartAndPro.ERP.SharedKernel.Dtos.DocumentShare;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers;

public class WhatsAppShareHandler<TId> : ShareHandler
{
    public override string Name => "WhatsApp";

    public DocumentShareResponseDto<TId> BuildLinks(IEnumerable<TId> ids, string receiver, string baseUrl)
    {
        if (ids == null || !ids.Any())
            throw new ArgumentException("At least one ID is required.", nameof(ids));

        if (string.IsNullOrWhiteSpace(receiver))
            throw new ArgumentException("Receiver phone number is required.", nameof(receiver));

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("BaseUrl is required.", nameof(baseUrl));

        var cleanReceiver = receiver.Replace("+", "").TrimStart('0');

        var idsQuery = string.Join("&id=", ids);
        var downloadLink = $"{baseUrl.TrimEnd('/')}?id={idsQuery}";

        var whatsappLink = $"https://wa.me/{cleanReceiver}?text={Uri.EscapeDataString(downloadLink)}";

        LogService.LogInformation(
            $"[WhatsAppShareHandler] Send this link via WhatsApp to {cleanReceiver}: {whatsappLink}");

        return new DocumentShareResponseDto<TId>
        {
            Method = "WhatsApp",
            Receiver = cleanReceiver,
            IsSuccess = true,
            DownloadLink = downloadLink,
            WhatsAppLink = whatsappLink,
            Message = "Links generated successfully"
        };
    }

    public override async Task Share(byte[] document, string receiver)
    {
        if (document == null || document.Length == 0)
            throw new ArgumentException("Document cannot be null or empty.", nameof(document));

        if (string.IsNullOrWhiteSpace(receiver))
            throw new ArgumentException("Receiver phone number is required.", nameof(receiver));

        var cleanReceiver = receiver.Replace("+", "").Replace(" ", "").TrimStart('0');
        
        // Ensure phone number has country code
        if (!cleanReceiver.StartsWith("966")) // Saudi Arabia
        {
            cleanReceiver = "966" + cleanReceiver;
        }

        // Save PDF to wwwroot/downloads folder for public access
        var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "downloads");
        Directory.CreateDirectory(webRootPath);

        var fileName = $"TrailBalance_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.pdf";
        var filePath = Path.Combine(webRootPath, fileName);

        await File.WriteAllBytesAsync(filePath, document);

        LogService.LogInformation($"[WhatsAppShareHandler] PDF saved to: {filePath}");

        // Generate public download link
        var downloadUrl = $"https://erp.smartapro.com/downloads/{fileName}";
        
        // Create WhatsApp message with download link
        var message = $"مرحباً،%0A%0Aتم إنشاء تقرير القيمة المضافة بنجاح.%0A%0Aلتحميل التقرير:%0A{downloadUrl}%0A%0Aحجم الملف: {document.Length / 1024} كيلوبايت";
        var whatsappLink = $"https://wa.me/{cleanReceiver}?text={message}";

        LogService.LogInformation($"[WhatsAppShareHandler] WhatsApp link: {whatsappLink}");
        LogService.LogInformation($"[WhatsAppShareHandler] Download URL: {downloadUrl}");
        LogService.LogInformation($"[WhatsAppShareHandler] File will be available for 24 hours");

        // Clean up old files (older than 24 hours)
        CleanOldFiles(webRootPath);

        await Task.CompletedTask;
    }

    private void CleanOldFiles(string folder)
    {
        try
        {
            var files = Directory.GetFiles(folder);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (DateTime.Now - fileInfo.CreationTime > TimeSpan.FromHours(24))
                {
                    File.Delete(file);
                    LogService.LogInformation($"[WhatsAppShareHandler] Deleted old file: {file}");
                }
            }
        }
        catch (Exception ex)
        {
            LogService.LogInformation($"[WhatsAppShareHandler] Failed to clean old files: {ex.Message}");
        }
    }
}
