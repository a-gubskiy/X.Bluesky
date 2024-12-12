using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace X.Bluesky.Tests;

public class BlueskyIntegrationTest
{
    [Fact(Skip = "On demand")]
    public async Task CheckFailedAuth()
    {
        var identifier = "devdigest.today";
        var password = "";

        var authorizationClient = new AuthorizationClient(identifier, password);

        var session = await authorizationClient.GetSession();

        Assert.NotNull(session);
    }
    
    [Fact(Skip = "On demand")]
    public async Task CheckSending()
    {
        var identifier = "devdigest.today";
        var password = "";
        
        IBlueskyClient client = new BlueskyClient(identifier, password);

        var link = new Uri("https://devdigest.today/post/2431");
        var text = $"Hello world! This post contains #devdigest and #microsoft also it include {link} which is in middle of post text and @andrew.gubskiy.com mention";
        
        await client.Post(text);
        
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