using System;
using System.Threading.Tasks;
using Xunit;

namespace X.Bluesky.Tests;

public class BlueskyIntegrationTest
{
    [Fact]
    public async Task CheckSending()
    {
        var identifier = "devdigest.bsky.social";
        var password = "{password-here}";
        
        IBlueskyClient client = new BlueskyClient(identifier, password);

        var link = new Uri("https://devdigest.today/post/2431");
        var text = $"Hello world! This post contains #devdigest and #microsoft also it include {link} which is in middle of post text and @andrew.gubskiy.com mention";
        
        await client.Post(text, link);
        
        Assert.True(true);
    }

    [Fact]
    public async Task TestResolveMention()
    {
        var mention = "@andrew.gubskiy.com";
        var httpClientFactory = new HttpClientFactory();
        
        IMentionResolver mentionResolver = new MentionResolver(httpClientFactory);
        
        var did = await mentionResolver.ResolveMention(mention);
        
        Assert.NotEmpty(did);
    }
}