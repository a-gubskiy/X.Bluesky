using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using X.Bluesky.EmbedCards;
using X.Bluesky.Models;
using X.Bluesky.Models.API;
using Xunit;

namespace X.Bluesky.Tests;

public class EmbedImageBuilderTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Session _session;
    private readonly Uri _baseUrl;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public EmbedImageBuilderTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger>();
        _session = new Session { AccessJwt = "test-token" };
        _baseUrl = new Uri("https://bsky.test");
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);
    }

    [Fact]
    public async Task UploadImage_WithValidImage_ReturnsThumb()
    {
        // Arrange
        var imageContent = new byte[] { 1, 2, 3, 4 };
        var mimeType = "image/jpeg";

        var responseContent = JsonConvert.SerializeObject(new BlobResponse
        {
            Blob = new Thumb
            {
                Ref = new ThumbRef
                {
                    Link = "test-ref"
                },
                Size = 100, MimeType = mimeType
            }
        });

        SetupMockHttpResponse(HttpStatusCode.OK, responseContent);

        var builder = new EmbedImageBuilder(_mockHttpClientFactory.Object, _session, _baseUrl, _mockLogger.Object);

        // Act
        var result = await builder.UploadImage(imageContent, mimeType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("blob", result.Type);
        Assert.Equal("test-ref", result.Ref.Link);
        Assert.Equal(100, result.Size);
        Assert.Equal(mimeType, result.MimeType);
    }

    [Fact]
    public async Task UploadImage_WithEmptyContent_ThrowsArgumentException()
    {
        // Arrange
        var imageContent = Array.Empty<byte>();
        var mimeType = "image/jpeg";

        var builder = new EmbedImageBuilder(_mockHttpClientFactory.Object, _session, _baseUrl, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => builder.UploadImage(imageContent, mimeType));
    }

    [Fact]
    public async Task UploadImage_WithTooLargeImage_ThrowsException()
    {
        // Arrange
        var imageContent = new byte[1000001]; // 1MB + 1 byte
        var mimeType = "image/jpeg";

        var builder = new EmbedImageBuilder(_mockHttpClientFactory.Object, _session, _baseUrl, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => builder.UploadImage(imageContent, mimeType));
        Assert.Contains("image file size too large", exception.Message);
    }

    [Fact]
    public async Task UploadImage_WhenServerReturnsError_ThrowsException()
    {
        // Arrange
        var imageContent = new byte[] { 1, 2, 3, 4 };
        var mimeType = "image/jpeg";

        SetupMockHttpResponse(HttpStatusCode.BadRequest, "Error");

        var builder = new EmbedImageBuilder(_mockHttpClientFactory.Object, _session, _baseUrl, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => builder.UploadImage(imageContent, mimeType));
    }

    [Fact]
    public async Task UploadImage_WhenBlobIsNull_ThrowsException()
    {
        // Arrange
        var imageContent = new byte[] { 1, 2, 3, 4 };
        var mimeType = "image/jpeg";

        var responseContent = JsonConvert.SerializeObject(new BlobResponse { Blob = null });

        SetupMockHttpResponse(HttpStatusCode.OK, responseContent);

        var builder = new EmbedImageBuilder(_mockHttpClientFactory.Object, _session, _baseUrl, _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => builder.UploadImage(imageContent, mimeType));
        Assert.Equal("Failed to upload image", exception.Message);
    }

    [Fact]
    public async Task GetEmbedCard_WithMultipleImages_ReturnsEmbedWithAllImages()
    {
        // Arrange
        var images = new List<Image>
        {
            new Image { Content = new byte[] { 1, 2, 3 }, MimeType = "image/jpeg", Alt = "Image 1" },
            new Image { Content = new byte[] { 4, 5, 6 }, MimeType = "image/png", Alt = "Image 2" }
        };

        var responseContent = JsonConvert.SerializeObject(new BlobResponse
        {
            Blob = new Thumb
            {
                Ref = new ThumbRef
                {
                    Link = "test-ref",
                },
                Size = 100, MimeType = "image/jpeg"
            }
        });

        SetupMockHttpResponse(HttpStatusCode.OK, responseContent);

        var builder = new EmbedImageBuilder(_mockHttpClientFactory.Object, _session, _baseUrl, _mockLogger.Object);

        // Act
        var result = await builder.GetEmbedCard(images) as EmbedImage;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Images.Count);
        Assert.Equal("Image 1", result.Images[0].Alt);
        Assert.Equal("Image 2", result.Images[1].Alt);
    }

    [Fact]
    public async Task GetEmbedCard_WithEmptyImageList_ReturnsEmptyEmbed()
    {
        // Arrange
        var images = new List<Image>();

        var builder = new EmbedImageBuilder(_mockHttpClientFactory.Object, _session, _baseUrl, _mockLogger.Object);

        // Act
        var result = await builder.GetEmbedCard(images) as EmbedImage;

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Images);
    }

    [Fact]
    public async Task GetEmbedCard_WithKnownSize_ReturnsEmbedWithAspectRatio()
    {
        // Arrange
        var images = new List<Image>
        {
            new Image { Content = new byte[] { 1, 2, 3 }, MimeType = "image/jpeg", Alt = "Image 1", Width = 800, Height = 600 },
            new Image { Content = new byte[] { 1, 2, 3 }, MimeType = "image/jpeg", Alt = "Image 2", Width = 800 },
        };

        var responseContent = JsonConvert.SerializeObject(new BlobResponse
        {
            Blob = new Thumb
            {
                Ref = new ThumbRef
                {
                    Link = "test-ref",
                },
                Size = 100,
                MimeType = "image/jpeg"
            }
        });

        SetupMockHttpResponse(HttpStatusCode.OK, responseContent);

        var builder = new EmbedImageBuilder(_mockHttpClientFactory.Object, _session, _baseUrl, _mockLogger.Object);

        // Act
        var result = await builder.GetEmbedCard(images) as EmbedImage;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Images.Count);
        Assert.Equal("Image 1", result.Images[0].Alt);
        Assert.IsType<AspectRatio>(result.Images[0].AspectRatio);
        Assert.Equal(800, result.Images[0].AspectRatio.Width);
        Assert.Equal(600, result.Images[0].AspectRatio.Height);
        Assert.Equal("Image 2", result.Images[1].Alt);
        Assert.Null(result.Images[1].AspectRatio);
    }

    private void SetupMockHttpResponse(HttpStatusCode statusCode, string content)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}
