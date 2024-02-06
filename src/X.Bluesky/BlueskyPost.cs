using Newtonsoft.Json;

namespace X.Bluesky;

public record BlueskyPost
{
    [JsonProperty("$type")] 
    public string Type { get; set; } = "";

    public string Text { get; set; } = "";

    public string CreatedAt { get; set; } = "";

    public BlueskyEmbed Embed { get; set; } = new();

    public List<string> Langs { get; set; } = new();

    public List<BlueskyFacet>? Facets { get; set; } = null;
}

public record BlueskyEmbed
{
    [JsonProperty("$type")] 
    public string Type { get; set; } = "";

    public BlueskyEmbedCard External { get; set; } = new();
}

public record BlueskyFacet
{
    public BlueskyFacetIndex Index { get; set; } = new();

    public List<BlueskyFacetFeature> Features { get; set; } = new();
}

public record BlueskyFacetFeature
{
    [JsonProperty("$type")] 
    public string Type { get; set; } = "";

    public Uri? Uri { get; set; }
}

public record BlueskyFacetIndex
{
    public int ByteStart { get; set; }

    public int ByteEnd { get; set; }
}