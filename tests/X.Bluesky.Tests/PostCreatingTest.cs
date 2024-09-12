using System;
using System.Threading.Tasks;
using Xunit;

namespace X.Bluesky.Tests;

public class PostCreatingTest
{
    [Fact]
    public async Task CheckFacets_Exist()
    {
        var facetBuilder = new FacetBuilder();

        var text = "This is a test and this is a #tag and https://example.com link and metions @one and @two in the end of text";
        
        var facets = facetBuilder.Create(text);

        Assert.Equal(4, facets.Count);
    }
    
    [Fact]
    public async Task CheckFacets_Empty()
    {
        var facetBuilder = new FacetBuilder();

        var text = "This is a test and this is a no link and no metions in the end of text";
        
        var facets = facetBuilder.Create(text);

        Assert.Empty(facets);
    }
}