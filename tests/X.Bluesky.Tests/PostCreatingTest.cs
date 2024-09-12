using System;
using System.Threading.Tasks;
using Xunit;

namespace X.Bluesky.Tests;

public class PostCreatingTest
{
    [Fact]
    public async Task CheckFacets()
    {
        var facetBuilder = new FacetBuilder();


        var text = "This is a test and this is a #tag and https://example.com link";
        var facets = facetBuilder.Create(text);

        Assert.Equal(2, facets.Count);
    }
}