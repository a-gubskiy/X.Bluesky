using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace X.Bluesky.Tests;

public class PostCreatingTest
{
    [Fact]
    public void CheckFacets_Exist()
    {
        var facetBuilder = new FacetBuilder();

        //var text = "This is a test and this is a #tag and https://example.com link and metions @one and @two in the end of text";
        var text = $"Hello world! This post contains #devdigest and #microsoft also it include https://devdigest.today which is in middle of post text and @andrew.gubskiy.com mention";
        
        var facets = facetBuilder.GetFacets(text);

        Assert.Equal(4, facets.Count);
    }
    
    [Fact]
    public void CheckFacetsMention()
    {
        var facetBuilder = new FacetBuilder();

        var text = "Hello world! This post contains #devdigest and #microsoft also it include https://devdigest.today which is in middle of post text and @andrew.gubskiy.com mention";

        var matches = facetBuilder.GetFeatureMentionMatches(text);
        var match = matches.ToList().FirstOrDefault();
        
        
        Assert.Equal("@andrew.gubskiy.com", match.Value);
    }
    
    [Fact]
    public void CheckFacets_Empty()
    {
        var facetBuilder = new FacetBuilder();

        var text = "This is a test and this is a no link and no metions in the end of text";
        
        var facets = facetBuilder.GetFacets(text);

        Assert.Empty(facets);
    }
}