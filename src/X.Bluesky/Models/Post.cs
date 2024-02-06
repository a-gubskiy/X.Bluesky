using Newtonsoft.Json;

namespace X.Bluesky.Models;

public record Post
{
    [JsonProperty("$type")]
    public string Type { get; set; } = "";

    public string Text { get; set; } = "";

    public string CreatedAt { get; set; } = "";

    public Embed Embed { get; set; } = new();

    public List<string> Langs { get; set; } = new();

    public List<Facet>? Facets { get; set; } = null;
}

public record Embed
{
    [JsonProperty("$type")]
    public string Type { get; set; } = "";

    public EmbedCard External { get; set; } = new();
}

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

public record FacetIndex
{
    public int ByteStart { get; set; }

    public int ByteEnd { get; set; }
}