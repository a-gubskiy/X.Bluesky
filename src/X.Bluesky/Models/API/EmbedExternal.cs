using Newtonsoft.Json;

namespace X.Bluesky.Models.API;

/// <summary>
/// External embed card
/// </summary>
internal record EmbedExternal : IEmbed
{
    [JsonProperty("$type")] public string Type => "app.bsky.embed.external";

    public External External { get; set; } = new();
}

internal record External
{
    public string Uri { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Thumb? Thumb { get; set; }
}