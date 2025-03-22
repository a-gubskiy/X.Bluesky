using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using X.Bluesky.Authorization;
using Xunit;

namespace X.Bluesky.Tests.Integration;

public class BlueskyIntegrationTest
{
    private const string Identifier = "";
    private const string Password = "";
    
    [Fact(Skip = "On demand")]
    public async Task CheckFailedAuth()
    {
        var authorizationClient = new AuthorizationClient(Identifier, Password);

        var session = await authorizationClient.GetSession();

        Assert.NotNull(session);
    }
    
    [Fact(Skip = "On demand")]
    public async Task CheckSending()
    {
        IBlueskyClient client = new BlueskyClient(Identifier, Password);

        var link = new Uri("https://devdigest.today/post/2431");
        var text = $"Hello world! This post contains #devdigest and #microsoft also it include {link} which is in middle of post text and @andrew.gubskiy.com mention";
        
        await client.Post(text);
        
        Assert.True(true);
    }
    
    [Fact(Skip = "On demand")]
    public async Task CheckSendingWithUrl()
    {
        IBlueskyClient client = new BlueskyClient(Identifier, Password);
        
        await client.Post("Testing!", new Uri("https://www.github.com"));
        await client.Post("Testing! https://www.github.com");
        await client.Post("Testing! www.github.com");
        
        Assert.True(true);
    }

    [Fact]
    public async Task TestResolveMention()
    {
        var mention = "@andrew.gubskiy.com";
        var httpClientFactory = new BlueskyHttpClientFactory();

        IMentionResolver mentionResolver = new MentionResolver(httpClientFactory, new Uri("https://bsky.social"), new NullLogger<MentionResolver>());
        
        var did = await mentionResolver.ResolveMention(mention);
        
        Assert.NotEmpty(did);
    }
}