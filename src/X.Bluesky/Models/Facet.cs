using Newtonsoft.Json;

namespace X.Bluesky.Models;

public record Facet
{
    public FacetIndex Index { get; set; } = new();

    public List<FacetFeature> Features { get; set; } = new();
}

public record FacetFeature
{
    [JsonProperty("$type")]
    public string Type { get; set; } = "";

    public Uri? Uri { get; set; }
}