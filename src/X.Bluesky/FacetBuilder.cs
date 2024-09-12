using System.Text.RegularExpressions;
using X.Bluesky.Models;

namespace X.Bluesky;

/// <summary>
/// 
/// </summary>
public class FacetBuilder
{
    public IReadOnlyCollection<Facet> Create(string text)
    {
        var result = new List<Facet>();

        var featureLinkMatches = GetFeatureLinkMatches(text);
        var featureMentionMatches = GetFeatureMentionMatches(text);
        var featureTagMatches = GetFeatureTagMatches(text);

        foreach (var match in featureLinkMatches)
        {
            var start = match.Index;
            var end = start + match.Length;

            result.Add(CreateFacet(start, end, new FacetFeatureLink { Uri = new Uri(match.Value) }));
        }

        foreach (var match in featureMentionMatches)
        {
            var start = match.Index;
            var end = start + match.Length;

            result.Add(CreateFacet(start, end, new FacetFeatureMention { Did = match.Value }));
        }

        foreach (var match in featureTagMatches)
        {
            var start = match.Index;
            var end = start + match.Length;
            var tag = match.Value.Replace("#", string.Empty);
            
            result.Add(CreateFacet(start, end, new FacetFeatureTag { Tag = tag }));
        }

        return result;
    }

    public Facet CreateFacet(int start, int end, FacetFeature facetFeature)
    {
        var result = new Facet
        {
            Index = new FacetIndex
            {
                ByteStart = start,
                ByteEnd = end
            },
            Features =
            [
                facetFeature
            ]
        };

        return result;
    }

    /// <summary>
    /// Detect hashtags
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IReadOnlyCollection<Match> GetFeatureTagMatches(string text)
    {
        var regex = new Regex(@"#\w+");
        var matches = regex.Matches(text).ToList();

        return matches;
    }

    /// <summary>
    /// Detect mentions
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IReadOnlyCollection<Match> GetFeatureMentionMatches(string text)
    {
        // var regex = new Regex(@"@\w+");
        var regex = new Regex(@"@\w+(\.\w+)*");
        var matches = regex.Matches(text).ToList();

        return matches;
    }

    /// <summary>
    /// Detect tags
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IReadOnlyCollection<Match> GetFeatureLinkMatches(string text)
    {
        var regex = new Regex(@"https?:\/\/[^\s]+");
        var matches = regex.Matches(text).ToList();

        return matches;
    }
}