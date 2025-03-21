using Newtonsoft.Json;

namespace X.Bluesky.Models.API;

internal abstract record FacetFeature
{
    [JsonProperty("$type")]
    public abstract string Type { get; }
}

internal record FacetFeatureLink : FacetFeature
{
    public Uri Uri { get; set; } = new Uri(string.Empty, UriKind.Relative);

    public override string Type => "app.bsky.richtext.facet#link";
}

internal record FacetFeatureMention : FacetFeature
{
    /// <summary>
    /// Important! Did must be resolved from @username to did:plc value
    /// </summary>
    public string Did { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public bool IsResolved => IsCorrectDid(Did);

    public void ResolveDid(string value)
    {
        if (!IsCorrectDid(value))
        {
            throw new FormatException("Did not recognize");
        }

        Did = value;
    }

    private bool IsCorrectDid(string did)
    {
        if (string.IsNullOrWhiteSpace(did))
        {
            return false;
        }

        if (did.Contains("did:plc:"))
        {
            return true;
        }

        return false;
    }

    public override string Type => "app.bsky.richtext.facet#mention";
}

internal record FacetFeatureTag : FacetFeature
{
    //tag: tag.replace(/^#/, ''),

    public string Tag { get; set; } = "";

    public override string Type => "app.bsky.richtext.facet#tag";
}