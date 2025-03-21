using System.Collections.Frozen;
using System.Text;
using System.Text.RegularExpressions;
using X.Bluesky.Models.API;

namespace X.Bluesky;

/// <summary>
/// Builds facets for Bluesky posts by detecting hashtags, mentions, and links in text.
/// Facets are metadata that describe special features within the text, such as mentions, links, or tags,
/// along with their byte positions in the UTF-8 encoded text.
/// </summary>
internal class FacetBuilder
{
    private readonly Regex _featureTagRegex;
    private readonly Regex _featureMentionRegex;
    private readonly Regex _featureLinkRegex;

    /// <summary>
    /// Initializes a new instance of the <see cref="FacetBuilder"/> class
    /// with compiled regular expressions for detecting hashtags, mentions, and links.
    /// </summary>
    public FacetBuilder()
    {
        _featureTagRegex = new Regex(@"#\w+", RegexOptions.Compiled);
        _featureMentionRegex = new Regex(@"@\w+(\.\w+)*", RegexOptions.Compiled);
        _featureLinkRegex = new Regex(@"https?:\/\/[\S]+", RegexOptions.Compiled);
    }

    /// <summary>
    /// Extracts facets from the given text by detecting hashtags, mentions, and links.
    /// </summary>
    /// <param name="text">The text content to extract facets from.</param>
    /// <returns>A collection of facets with their byte positions and features.</returns>
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

    /// <summary>
    /// Creates a facet with the given byte positions and feature.
    /// </summary>
    /// <param name="start">The starting byte position in UTF-8 encoding.</param>
    /// <param name="end">The ending byte position in UTF-8 encoding.</param>
    /// <param name="facetFeature">The feature to associate with this facet (link, mention, or tag).</param>
    /// <returns>A new facet with the specified properties.</returns>
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
    /// Finds all hashtags in the given text.
    /// </summary>
    /// <param name="text">The text to search for hashtags.</param>
    /// <returns>A collection of regex matches for hashtags.</returns>
    private IReadOnlyCollection<Match> GetFeatureTagMatches(string text)
    {
        var matches = _featureTagRegex.Matches(text).ToFrozenSet();

        return matches;
    }

    /// <summary>
    /// Finds all mentions in the given text.
    /// </summary>
    /// <param name="text">The text to search for mentions.</param>
    /// <returns>A collection of regex matches for mentions.</returns>
    internal IReadOnlyCollection<Match> GetFeatureMentionMatches(string text)
    {
        var matches = _featureMentionRegex.Matches(text).ToFrozenSet();

        return matches;
    }

    /// <summary>
    /// Finds all links in the given text.
    /// </summary>
    /// <param name="text">The text to search for links.</param>
    /// <returns>A collection of regex matches for links.</returns>
    private IReadOnlyCollection<Match> GetFeatureLinkMatches(string text)
    {
        var matches = _featureLinkRegex.Matches(text).ToFrozenSet();

        return matches;
    }

    /// <summary>
    /// Calculates the byte position of a character in the UTF-8 encoded version of the text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <param name="index">The character index in the string.</param>
    /// <returns>The byte position in the UTF-8 encoded text.</returns>
    /// <remarks>
    /// This is needed because Bluesky's API requires byte positions in UTF-8 encoding,
    /// but C# strings are UTF-16 encoded.
    /// </remarks>
    private int GetUtf8BytePosition(string text, int index)
    {
        var substring = text[..index];

        return Encoding.UTF8.GetByteCount(substring);
    }
}