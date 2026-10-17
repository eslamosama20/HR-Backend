using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Cloudinary;

public class CloudinaryServiceHelperTests
{
    private static readonly Type CloudinaryServiceType = typeof(SmartAndPro.ERP.WebAPI.Services.Cloudinary.CloudinaryService);
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

    #region DetectContentType Tests

    [Theory]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A }, "image/png")]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, "image/jpeg")]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, "image/jpeg")]
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, "image/gif")]
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, "image/gif")]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 }, "image/webp")]
    public void DetectContentType_KnownMagicBytes_ReturnsCorrectType(byte[] bytes, string expected)
    {
        var method = CloudinaryServiceType.GetMethod("DetectContentType", PrivateStatic);
        var result = method!.Invoke(null, [bytes]);
        result.Should().Be(expected);
    }

    [Fact]
    public void DetectContentType_UnknownBytes_Throws()
    {
        var method = CloudinaryServiceType.GetMethod("DetectContentType", PrivateStatic);
        Action act = () => method!.Invoke(null, [new byte[] { 0x00, 0x00, 0x00, 0x00 }]);
        act.Should().Throw<TargetInvocationException>();
    }

    [Fact]
    public void DetectContentType_EmptyBytes_Throws()
    {
        var method = CloudinaryServiceType.GetMethod("DetectContentType", PrivateStatic);
        Action act = () => method!.Invoke(null, [Array.Empty<byte>()]);
        act.Should().Throw<TargetInvocationException>();
    }

    [Fact]
    public void DetectContentType_FewerThan4Bytes_Throws()
    {
        var method = CloudinaryServiceType.GetMethod("DetectContentType", PrivateStatic);
        Action act = () => method!.Invoke(null, [new byte[] { 0xFF, 0xD8 }]);
        act.Should().Throw<TargetInvocationException>();
    }

    #endregion

    #region ValidateFile Tests

    [Fact]
    public void ValidateFile_NullFile_ThrowsInvalidOperation()
    {
        var method = CloudinaryServiceType.GetMethod("ValidateFile", PrivateStatic);

        var action = () => method!.Invoke(null, [(IFormFile?)null!]);

        action.Should().Throw<TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("File is required.");
    }

    [Fact]
    public void ValidateFile_EmptyFile_ThrowsInvalidOperation()
    {
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(0);

        var method = CloudinaryServiceType.GetMethod("ValidateFile", PrivateStatic);
        var action = () => method!.Invoke(null, [file.Object]);

        action.Should().Throw<TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("File is required.");
    }

    [Fact]
    public void ValidateFile_Over5Mb_ThrowsInvalidOperation()
    {
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(6 * 1024 * 1024);
        file.Setup(f => f.ContentType).Returns("image/jpeg");

        var method = CloudinaryServiceType.GetMethod("ValidateFile", PrivateStatic);
        var action = () => method!.Invoke(null, [file.Object]);

        action.Should().Throw<TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("File size must be 5 MB or less.");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    public void ValidateFile_AllowedTypes_DoesNotThrow(string contentType)
    {
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(1024);
        file.Setup(f => f.ContentType).Returns(contentType);

        var method = CloudinaryServiceType.GetMethod("ValidateFile", PrivateStatic);
        var action = () => method!.Invoke(null, [file.Object]);

        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/gif")]
    [InlineData("image/bmp")]
    [InlineData("text/plain")]
    [InlineData("application/octet-stream")]
    public void ValidateFile_DisallowedTypes_ThrowsInvalidOperation(string contentType)
    {
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(1024);
        file.Setup(f => f.ContentType).Returns(contentType);

        var method = CloudinaryServiceType.GetMethod("ValidateFile", PrivateStatic);
        var action = () => method!.Invoke(null, [file.Object]);

        action.Should().Throw<TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("Unsupported file type.");
    }

    #endregion

    #region DecodeBase64 Tests

    [Fact]
    public void DecodeBase64_NullInput_ReturnsNull()
    {
        var method = typeof(SmartAndPro.ERP.WebAPI.Controllers.CloudinaryMigrationController)
            .GetMethod("DecodeBase64", PrivateStatic);
        var result = method!.Invoke(null, [(string?)null]);
        result.Should().BeNull();
    }

    [Fact]
    public void DecodeBase64_EmptyString_ReturnsNull()
    {
        var method = typeof(SmartAndPro.ERP.WebAPI.Controllers.CloudinaryMigrationController)
            .GetMethod("DecodeBase64", PrivateStatic);
        var result = method!.Invoke(null, [""]);
        result.Should().BeNull();
    }

    [Fact]
    public void DecodeBase64_WhitespaceString_ReturnsNull()
    {
        var method = typeof(SmartAndPro.ERP.WebAPI.Controllers.CloudinaryMigrationController)
            .GetMethod("DecodeBase64", PrivateStatic);
        var result = method!.Invoke(null, ["   "]);
        result.Should().BeNull();
    }

    [Fact]
    public void DecodeBase64_ValidBase64_ReturnsBytes()
    {
        var method = typeof(SmartAndPro.ERP.WebAPI.Controllers.CloudinaryMigrationController)
            .GetMethod("DecodeBase64", PrivateStatic);
        var plainBytes = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        var base64 = Convert.ToBase64String(plainBytes);
        var result = method!.Invoke(null, [base64]);
        result.Should().BeOfType<byte[]>();
        ((byte[])result!).Should().BeEquivalentTo(plainBytes);
    }

    [Fact]
    public void DecodeBase64_ValidBase64WithDataUriPrefix_ReturnsBytes()
    {
        var method = typeof(SmartAndPro.ERP.WebAPI.Controllers.CloudinaryMigrationController)
            .GetMethod("DecodeBase64", PrivateStatic);
        var plainBytes = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        var base64 = Convert.ToBase64String(plainBytes);
        var result = method!.Invoke(null, [$"data:image/png;base64,{base64}"]);
        result.Should().BeOfType<byte[]>();
        ((byte[])result!).Should().BeEquivalentTo(plainBytes);
    }

    [Fact]
    public void DecodeBase64_Base64WithWhitespace_ReturnsBytes()
    {
        var method = typeof(SmartAndPro.ERP.WebAPI.Controllers.CloudinaryMigrationController)
            .GetMethod("DecodeBase64", PrivateStatic);
        var plainBytes = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        var base64 = Convert.ToBase64String(plainBytes);
        var withPadding = base64 + "\r\n ";
        var result = method!.Invoke(null, [withPadding]);
        result.Should().BeOfType<byte[]>();
        ((byte[])result!).Should().BeEquivalentTo(plainBytes);
    }

    [Fact]
    public void DecodeBase64_MalformedBase64_ReturnsNull()
    {
        var method = typeof(SmartAndPro.ERP.WebAPI.Controllers.CloudinaryMigrationController)
            .GetMethod("DecodeBase64", PrivateStatic);
        var result = method!.Invoke(null, ["not-valid-base64!!!@#$%"]);
        result.Should().BeNull();
    }

    [Fact]
    public void DecodeBase64_Base64WithLineBreaks_ReturnsBytes()
    {
        var method = typeof(SmartAndPro.ERP.WebAPI.Controllers.CloudinaryMigrationController)
            .GetMethod("DecodeBase64", PrivateStatic);
        var plainBytes = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        var base64 = Convert.ToBase64String(plainBytes);
        var withLineBreaks = base64.Insert(3, "\n").Insert(5, "\r");
        var result = method!.Invoke(null, [withLineBreaks]);
        result.Should().BeOfType<byte[]>();
        ((byte[])result!).Should().BeEquivalentTo(plainBytes);
    }

    #endregion
}
