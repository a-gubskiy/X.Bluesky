using Newtonsoft.Json;

namespace X.Bluesky.Models;

public record Facet
{
    public FacetIndex Index { get; set; } = new();

    public List<FacetFeature> Features { get; set; } = new();
}

public abstract record FacetFeature
{
    [JsonProperty("$type")]
    public abstract string Type { get; }
}

public record FacetFeatureLink : FacetFeature
{
    public Uri Uri { get; set; }
    public override string Type => "app.bsky.richtext.facet#link";
}

public record FacetFeatureMention : FacetFeature
{
    //did: match[3], // must be resolved afterwards
    
    public string Did { get; set; }
    
    public override string Type => "app.bsky.richtext.facet#mention";
}

public record FacetFeatureTag : FacetFeature
{
    //tag: tag.replace(/^#/, ''),

    public string Tag { get; set; }

    public override string Type => "app.bsky.richtext.facet#tag";
}