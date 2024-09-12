using System;
using System.Threading.Tasks;
using Xunit;

namespace X.Bluesky.Tests;

public class PostCreatingTest
{
    [Fact]
    public async Task CheckFacets()
    {
                
        var client = new BlueskyClient("identifier", "password");

        var text = "This is a test and this is a #tag and https://example.com link";
        var facets = await client.GetFacets(text);

        Assert.Equal(2, facets.Count);
    }
}