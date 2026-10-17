using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Utility;

public static class ImageDownloader
{
    private static readonly ConcurrentDictionary<string, byte[]> ImageCache = new();
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static async Task<byte[]?> DownloadImageBytesAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        if (ImageCache.TryGetValue(url, out var cachedBytes))
        {
            return cachedBytes;
        }

        try
        {
            var bytes = await HttpClient.GetByteArrayAsync(url);
            if (bytes != null && bytes.Length > 0)
            {
                ImageCache[url] = bytes;
                return bytes;
            }
        }
        catch (Exception ex)
        {
            // Soft-fail: log the error or write to debug console
            System.Diagnostics.Debug.WriteLine($"Failed to download image from {url}: {ex.Message}");
        }

        return null;
    }
}
