using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace X.Bluesky.Tests;

public class FileTypeHelperTests
{
    private readonly FileTypeHelper _fileTypeHelper;

    public FileTypeHelperTests()
    {
        var loggerMock = new Mock<ILogger>();
        
        _fileTypeHelper = new FileTypeHelper(loggerMock.Object);
    }

    [Fact]
    public void GetFileExtensionFromUrl_ValidUrlWithExtension_ReturnsExtension()
    {
        var url = "https://example.com/image.jpg";
        var result = _fileTypeHelper.GetFileExtensionFromUrl(url);
        Assert.Equal(".jpg", result);
    }

    [Fact]
    public void GetFileExtensionFromUrl_ValidUrlWithoutExtension_ReturnsEmptyString()
    {
        var url = "https://example.com/path";
        var result = _fileTypeHelper.GetFileExtensionFromUrl(url);
        
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetFileExtensionFromUrl_UrlWithDotInPath_ReturnsCorrectExtension()
    {
        var url = "https://example.com/path.with.dots/file.png";
        var result = _fileTypeHelper.GetFileExtensionFromUrl(url);
        
        Assert.Equal(".png", result);
    }

    [Fact]
    public void GetFileExtensionFromUrl_InvalidUrl_ReturnsEmptyString()
    {
        var url = "not a valid url";
        var result = _fileTypeHelper.GetFileExtensionFromUrl(url);
        
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetFileExtensionFromUrl_NullUrl_ReturnsEmptyString()
    {
        string url = null;
        var result = _fileTypeHelper.GetFileExtensionFromUrl(url);
        
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetFileExtensionFromUrl_UrlWithQueryString_IgnoresQueryString()
    {
        var url = "https://example.com/image.jpg?width=100&height=100";
        var result = _fileTypeHelper.GetFileExtensionFromUrl(url);
        
        Assert.Equal(".jpg", result);
    }

    [Fact]
    public void GetMimeTypeFromUrl_UrlWithJpgExtension_ReturnsJpegMimeType()
    {
        var url = "https://example.com/image.jpg";
        var result = _fileTypeHelper.GetMimeTypeFromUrl(url);
        
        Assert.Equal("image/jpeg", result);
    }

    [Fact]
    public void GetMimeTypeFromUrl_UrlWithPngExtension_ReturnsPngMimeType()
    {
        var url = "https://example.com/image.png";
        var result = _fileTypeHelper.GetMimeTypeFromUrl(url);
        
        Assert.Equal("image/png", result);
    }

    [Fact]
    public void GetMimeTypeFromUrl_UrlWithUnknownExtension_ReturnsOctetStreamMimeType()
    {
        var url = "https://example.com/file.xyz";
        var result = _fileTypeHelper.GetMimeTypeFromUrl(url);
        
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void GetMimeTypeFromUrl_UrlWithoutExtension_ReturnsOctetStreamMimeType()
    {
        var url = "https://example.com/path";
        var result = _fileTypeHelper.GetMimeTypeFromUrl(url);
        
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void GetMimeTypeFromUri_UriWithJpgExtension_ReturnsJpegMimeType()
    {
        var uri = new Uri("https://example.com/image.jpg");
        var result = _fileTypeHelper.GetMimeTypeFromUrl(uri);
        
        Assert.Equal("image/jpeg", result);
    }

    [Fact]
    public void GetMimeTypeFromUri_UriWithoutExtension_ReturnsOctetStreamMimeType()
    {
        var uri = new Uri("https://example.com/path");
        var result = _fileTypeHelper.GetMimeTypeFromUrl(uri);
        
        Assert.Equal("application/octet-stream", result);
    }

    [Theory]
    [InlineData(".jpg", "image/jpeg")]
    [InlineData(".jpeg", "image/jpeg")]
    [InlineData(".png", "image/png")]
    [InlineData(".gif", "image/gif")]
    [InlineData(".webp", "image/webp")]
    [InlineData(".svg", "image/svg+xml")]
    [InlineData(".tiff", "image/tiff")]
    [InlineData(".tif", "image/tiff")]
    [InlineData(".avif", "image/avif")]
    [InlineData(".heic", "image/heic")]
    [InlineData(".bmp", "image/bmp")]
    [InlineData(".ico", "image/x-icon")]
    [InlineData(".icon", "image/x-icon")]
    [InlineData(".xyz", "application/octet-stream")]
    public void GetMimeType_VariousExtensions_ReturnsCorrectMimeType(string extension, string expectedMimeType)
    {
        var result = _fileTypeHelper.GetMimeType(extension);
        Assert.Equal(expectedMimeType, result);
    }

    [Fact]
    public void GetMimeType_NullExtension_ReturnsOctetStreamMimeType()
    {
        string extension = null;
        var result = _fileTypeHelper.GetMimeType(extension);
        
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void GetMimeType_EmptyExtension_ReturnsOctetStreamMimeType()
    {
        var extension = string.Empty;
        var result = _fileTypeHelper.GetMimeType(extension);
        
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void GetMimeType_ExtensionWithoutDot_AddsDotAndReturnsCorrectMimeType()
    {
        var extension = "jpg";
        var result = _fileTypeHelper.GetMimeType(extension);
        
        Assert.Equal("image/jpeg", result);
    }

    [Fact]
    public void GetMimeType_UppercaseExtension_IgnoresCaseAndReturnsCorrectMimeType()
    {
        var extension = ".JPG";
        var result = _fileTypeHelper.GetMimeType(extension);
        
        Assert.Equal("image/jpeg", result);
    }
}