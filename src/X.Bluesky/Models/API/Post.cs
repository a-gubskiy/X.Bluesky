using Newtonsoft.Json;

namespace X.Bluesky.Models.API;

internal record Post
{
    [JsonProperty("$type")]
    public string Type { get; set; } = "";

    public string Text { get; set; } = "";

    public string CreatedAt { get; set; } = "";

    public IEmbed? Embed { get; set; } = null;

    public List<string> Langs { get; set; } = new();

    public List<Facet>? Facets { get; set; } = null;
}

internal record FacetIndex
{
    public int ByteStart { get; set; }

    public int ByteEnd { get; set; }
}