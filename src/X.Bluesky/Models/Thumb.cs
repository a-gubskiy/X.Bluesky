using Newtonsoft.Json;

namespace X.Bluesky.Models;

public record Thumb
{
    [JsonProperty("$type")]
    public string Type { get; set; } = "";

    [JsonProperty("ref")]
    public ThumbRef? Ref { get; set; }

    [JsonProperty("mimeType")]
    public string MimeType { get; set; } = "";

    [JsonProperty("size")]
    public int Size { get; set; }
}

public class ThumbRef
{
    [JsonProperty("$link")]
    public string Link { get; set; } = "";
}