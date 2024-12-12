using Newtonsoft.Json;

namespace X.Bluesky.Models;

public interface IEmbed
{
}

public record Embed : IEmbed
{
    [JsonProperty("$type")]
    public string Type { get; set; } = "";

    public EmbedCard External { get; set; } = new();
}