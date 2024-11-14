using System.Text;
using System.Text.RegularExpressions;
using X.Bluesky.Models;

namespace X.Bluesky;

/// <summary>
/// FacetBuilder class to extract facets from the text for BlueSky API.
/// </summary>
public class FacetBuilder
{
    /// <summary>
    /// Get facets from the text
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IReadOnlyCollection<Facet> GetFacets(string text)
    {
        var result = new List<Facet>();

        var featureLinkMatches = GetFeatureLinkMatches(text);
        var featureMentionMatches = GetFeatureMentionMatches(text);
        var featureTagMatches = GetFeatureTagMatches(text);

        foreach (var match in featureLinkMatches)
        {
            var start = GetUtf8BytePosition(text, match.Index);
            var end = GetUtf8BytePosition(text, match.Index + match.Length);

            result.Add(CreateFacet(start, end, new FacetFeatureLink { Uri = new Uri(match.Value) }));
        }

        foreach (var match in featureMentionMatches)
        {
            var start = GetUtf8BytePosition(text, match.Index);
            var end = GetUtf8BytePosition(text, match.Index + match.Length);

            result.Add(CreateFacet(start, end, new FacetFeatureMention { Did = match.Value }));
        }

        foreach (var match in featureTagMatches)
        {
            var start = GetUtf8BytePosition(text, match.Index);
            var end = GetUtf8BytePosition(text, match.Index + match.Length);
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
            Features = [facetFeature]
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
        var regex = new Regex(@"@\w+(\.\w+)*");
        var matches = regex.Matches(text).ToList();

        return matches;
    }

    /// <summary>
    /// Detect links
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IReadOnlyCollection<Match> GetFeatureLinkMatches(string text)
    {
        var regex = new Regex(@"https?:\/\/[\S]+", RegexOptions.Compiled);
        var matches = regex.Matches(text).ToList();

        return matches;
    }

    /// <summary>
    /// Convert character index to UTF-8 byte index.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="index">The character index in the text.</param>
    /// <returns>The corresponding UTF-8 byte index.</returns>
    private int GetUtf8BytePosition(string text, int index)
    {
        var substring = text[..index];
        
        return Encoding.UTF8.GetByteCount(substring);
    }
}
