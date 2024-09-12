using X.Bluesky.Models;

namespace X.Bluesky;

public class FacetBuilder
{
    public FacetFeature? Create(string text)
    {
        FacetFeature? facetFeature = null;

        if (IsFacetFeatureLink(text))
        {
            facetFeature = CreateFacetFeatureLink();
        }
        else if (IsFacetFeatureMention(text))
        {
            facetFeature = CreateFacetFeatureMention();
        }
        else if (IsFacetFeatureTag(text))
        {
            facetFeature = CreateFacetFeatureTag();
        }

        return facetFeature;
    }

    private bool IsFacetFeatureTag(string text)
    {
        throw new NotImplementedException();
    }

    private bool IsFacetFeatureMention(string text)
    {
        throw new NotImplementedException();
    }

    private bool IsFacetFeatureLink(string text)
    {
        throw new NotImplementedException();
    }

    private static FacetFeatureTag CreateFacetFeatureTag()
    {
        return new FacetFeatureTag();
    }

    private static FacetFeatureMention CreateFacetFeatureMention()
    {
        return new FacetFeatureMention();
    }

    private static FacetFeatureLink CreateFacetFeatureLink()
    {
        return new FacetFeatureLink();
    }
    
    // public static List<Facet> DetectFacets(string text)
    // {
    //     var facets = new List<Facet>();
    //
    //     // Detect links (http/https URLs)
    //     var linkRegex = new Regex(@"https?:\/\/[^\s]+");
    //     foreach (Match match in linkRegex.Matches(text))
    //     {
    //         facets.Add(new Facet
    //         {
    //             Index = new Index
    //             {
    //                 ByteStart = match.Index,
    //                 ByteEnd = match.Index + match.Length
    //             },
    //             Features = new List<Feature>
    //             {
    //                 new Feature
    //                 {
    //                     Type = "app.bsky.richtext.facet#link",
    //                     Uri = match.Value
    //                 }
    //             }
    //         });
    //     }
    //
    //     // Detect hashtags (#tag)
    //     var tagRegex = new Regex(@"#\w+");
    //     foreach (Match match in tagRegex.Matches(text))
    //     {
    //         facets.Add(new Facet
    //         {
    //             Index = new Index
    //             {
    //                 ByteStart = match.Index,
    //                 ByteEnd = match.Index + match.Length
    //             },
    //             Features = new List<Feature>
    //             {
    //                 new Feature
    //                 {
    //                     Type = "app.bsky.richtext.facet#tag",
    //                     Tag = match.Value
    //                 }
    //             }
    //         });
    //     }
    //
    //     // Detect mentions (@username)
    //     var mentionRegex = new Regex(@"@\w+");
    //     foreach (Match match in mentionRegex.Matches(text))
    //     {
    //         facets.Add(new Facet
    //         {
    //             Index = new Index
    //             {
    //                 ByteStart = match.Index,
    //                 ByteEnd = match.Index + match.Length
    //             },
    //             Features = new List<Feature>
    //             {
    //                 new Feature
    //                 {
    //                     Type = "app.bsky.richtext.facet#mention",
    //                     Did = match.Value
    //                 }
    //             }
    //         });
    //     }
    //
    //     return facets;
    // }
}

