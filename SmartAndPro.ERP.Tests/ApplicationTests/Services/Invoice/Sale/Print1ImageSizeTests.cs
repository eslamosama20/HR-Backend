using System;
using FluentAssertions;
using SmartAndPro.ERP.Application.Services.Invoice.Sale.DocumentUtils;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Invoice.Sale;

/// <summary>
/// Tests for the Print-1 logo size guard (Print1ImageSize.TryGetImageSize):
/// PNG/JPEG dimension reads from raw headers + safe fallbacks, so a portrait or
/// huge logo can never render unbounded inside the header (the QR/stamp already
/// use fixed FitArea boxes; the logo now gets an aspect-aware cap).
/// </summary>
public class Print1ImageSizeTests
{
    [Fact]
    public void TryGetImageSize_PngHeader_ReturnsDimensions()
    {
        // Minimal 24-byte PNG buffer: 8-byte signature + IHDR width/height (big-endian).
        var data = new byte[24];
        data[0] = 0x89; data[1] = 0x50; data[2] = 0x4E; data[3] = 0x47; // PNG signature
        data[16] = 0x00; data[17] = 0x00; data[18] = 0x01; data[19] = 0x2C; // width  = 300
        data[20] = 0x00; data[21] = 0x00; data[22] = 0x00; data[23] = 0x96; // height = 150

        var (w, h) = Print1ImageSize.TryGetImageSize(data);

        w.Should().Be(300);
        h.Should().Be(150);
    }

    [Fact]
    public void TryGetImageSize_PortraitPng_ReturnsTallDimensions()
    {
        var data = new byte[24];
        data[0] = 0x89; data[1] = 0x50; data[2] = 0x4E; data[3] = 0x47;
        data[16] = 0x00; data[17] = 0x00; data[18] = 0x01; data[19] = 0xF4; // width  = 500
        data[20] = 0x00; data[21] = 0x00; data[22] = 0x07; data[23] = 0xD0; // height = 2000

        var (w, h) = Print1ImageSize.TryGetImageSize(data);

        w.Should().Be(500);
        h.Should().Be(2000);
    }

    [Fact]
    public void TryGetImageSize_JpegHeader_ReturnsDimensions()
    {
        // SOI + APP0 segment (len 16) + SOF0 marker carrying dimensions.
        var data = new byte[]
        {
            0xFF, 0xD8,             // SOI
            0xFF, 0xE0, 0x00, 0x10, // APP0, length 16
            0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00,
            0xFF, 0xC0, 0x00, 0x11, // SOF0, length 17
            0x08,                   // precision
            0x00, 0x64,             // height = 100
            0x00, 0xC8,             // width  = 200
            0x03, 0x01, 0x22, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01
        };

        var (w, h) = Print1ImageSize.TryGetImageSize(data);

        w.Should().Be(200);
        h.Should().Be(100);
    }

    [Fact]
    public void TryGetImageSize_UnknownOrShortData_ReturnsNulls()
    {
        var (w1, h1) = Print1ImageSize.TryGetImageSize(new byte[] { 0x01, 0x02, 0x03 });
        w1.Should().BeNull();
        h1.Should().BeNull();

        var (w2, h2) = Print1ImageSize.TryGetImageSize(new byte[24]); // zeroed header
        w2.Should().BeNull();
        h2.Should().BeNull();

        var (w3, h3) = Print1ImageSize.TryGetImageSize(null!);
        w3.Should().BeNull();
        h3.Should().BeNull();
    }
}
