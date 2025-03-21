using System.Collections.Frozen;
using System.Text;
using System.Text.RegularExpressions;
using X.Bluesky.Models.API;

namespace X.Bluesky;

/// <summary>
/// FacetBuilder class to extract facets from the text for BlueSky API.
/// </summary>
internal class FacetBuilder
{
    private readonly Regex _featureTagRegex;
    private readonly Regex _featureMentionRegex;
    private readonly Regex _featureLinkRegex;

    /// <summary>
    /// Create a new instance of FacetBuilder
    /// </summary>
    public FacetBuilder()
    {
        _featureTagRegex = new Regex(@"#\w+", RegexOptions.Compiled);
        _featureMentionRegex = new Regex(@"@\w+(\.\w+)*", RegexOptions.Compiled);
        _featureLinkRegex = new Regex(@"https?:\/\/[\S]+", RegexOptions.Compiled);
    }

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

    private Facet CreateFacet(int start, int end, FacetFeature facetFeature)
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
    private  IReadOnlyCollection<Match> GetFeatureTagMatches(string text)
    {
        var matches = _featureTagRegex.Matches(text).ToFrozenSet();

        return matches;
    }

    /// <summary>
    /// Detect mentions
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    internal  IReadOnlyCollection<Match> GetFeatureMentionMatches(string text)
    {
        var matches = _featureMentionRegex.Matches(text).ToFrozenSet();

        return matches;
    }

    /// <summary>
    /// Detect links
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private  IReadOnlyCollection<Match> GetFeatureLinkMatches(string text)
    {
        var matches = _featureLinkRegex.Matches(text).ToFrozenSet();

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