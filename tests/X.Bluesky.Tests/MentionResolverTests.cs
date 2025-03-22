using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;

namespace X.Bluesky.Tests;

public class MentionResolverTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Uri _baseUrl = new("https://bsky.social");
    private readonly MentionResolver _resolver;

    public MentionResolverTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger>();

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _resolver = new MentionResolver(mockHttpClientFactory.Object, _baseUrl, _mockLogger.Object);
    }

    [Fact]
    public async Task ResolveMention_WithValidHandle_ReturnsDid()
    {
        var handle = "user.bsky.social";
        var expectedDid = "did:plc:abcdefghijklmnop";

        SetupHttpResponse(HttpStatusCode.OK, new JObject { ["did"] = expectedDid }.ToString());

        var result = await _resolver.ResolveMention(handle);

        Assert.Equal(expectedDid, result);
        VerifyHttpRequest($"https://bsky.social/xrpc/com.atproto.identity.resolveHandle?handle=user.bsky.social");
    }

    [Fact]
    public async Task ResolveMention_WithAtPrefixedHandle_RemovesPrefixAndReturnsDid()
    {
        var handle = "@user.bsky.social";
        var expectedDid = "did:plc:qrstuvwxyz";

        SetupHttpResponse(HttpStatusCode.OK, new JObject { ["did"] = expectedDid }.ToString());

        var result = await _resolver.ResolveMention(handle);

        Assert.Equal(expectedDid, result);
        VerifyHttpRequest($"https://bsky.social/xrpc/com.atproto.identity.resolveHandle?handle=user.bsky.social");
    }

    [Fact]
    public async Task ResolveMention_WhenApiReturnsError_ReturnsEmptyString()
    {
        var handle = "nonexistent.bsky.social";

        SetupHttpResponse(HttpStatusCode.NotFound, "Not Found");

        var result = await _resolver.ResolveMention(handle);

        Assert.Equal(string.Empty, result);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error: Not Found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResolveMention_WhenDidIsMissingInResponse_ReturnsEmptyString()
    {
        var handle = "user.bsky.social";

        SetupHttpResponse(HttpStatusCode.OK, new JObject { ["other"] = "value" }.ToString());

        var result = await _resolver.ResolveMention(handle);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task ResolveMention_WithEmptyHandle_SendsEmptyHandleParameter()
    {
        var handle = "";

        SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");

        var result = await _resolver.ResolveMention(handle);

        Assert.Equal(string.Empty, result);
        VerifyHttpRequest($"https://bsky.social/xrpc/com.atproto.identity.resolveHandle?handle=");
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
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

    private void VerifyHttpRequest(string expectedRequestUri)
    {
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() == expectedRequestUri),
                ItExpr.IsAny<CancellationToken>());
    }
}