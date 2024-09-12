using System.Text.RegularExpressions;
using X.Bluesky.Models;

namespace X.Bluesky;

public class FacetBuilder
{
    public IReadOnlyCollection<Facet> Create(string text)
    {
        var result = new List<Facet>();
        // FacetFeature? facetFeature = null;

        var featureLinkMatches = GetFeatureLinkMatches(text);
        var featureMentionMatches = GetFeatureMentionMatches(text);
        var featureTagMatches = GetFeatureTagMatches(text);

        foreach (var match in featureLinkMatches)
        {
            result.Add(new Facet
            {
                Index = new FacetIndex
                {
                    ByteStart = match.Index,
                    ByteEnd = match.Index + match.Length
                },
                Features =
                [
                    new FacetFeatureLink { Uri = new Uri(match.Value) }
                ]
            });
        }

        foreach (var match in featureMentionMatches)
        {
            result.Add(new Facet
            {
                Index = new FacetIndex
                {
                    ByteStart = match.Index,
                    ByteEnd = match.Index + match.Length
                },
                Features =
                [
                    new FacetFeatureMention { Did = match.Value }
                ]
            });
        }

        foreach (var match in featureTagMatches)
        {
            result.Add(new Facet
            {
                Index = new FacetIndex
                {
                    ByteStart = match.Index,
                    ByteEnd = match.Index + match.Length
                },
                Features =
                [
                    new FacetFeatureTag { Tag = match.Value }
                ]
            });
        }

        return result;
    }

    /// <summary>
    /// Detect hashtags
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private IReadOnlyCollection<Match> GetFeatureTagMatches(string text)
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
    private IReadOnlyCollection<Match> GetFeatureMentionMatches(string text)
    {
        var regex = new Regex(@"@\w+");
        var matches = regex.Matches(text).ToList();

        return matches;
    }

    /// <summary>
    /// Detect tags
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private IReadOnlyCollection<Match> GetFeatureLinkMatches(string text)
    {
        var regex = new Regex(@"https?:\/\/[^\s]+");
        var matches = regex.Matches(text).ToList();

        return matches;
    }
}