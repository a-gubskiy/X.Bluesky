using System;
using System.Threading.Tasks;
using X.Bluesky;
using Xunit;

namespace Tests;

public class BlueskyIntegrationTest
{
    [Fact]
    public async Task CheckSending()
    {
        var identifier = "devdigest.bsky.social";
        var password = "{password-here}";
        
        IBlueskyClient client = new BlueskyClient(identifier, password);

        var link = new Uri("https://devdigest.today/post/2431");

        await client.Post("Hello world!", link);
        
        Assert.True(true);
    }
}