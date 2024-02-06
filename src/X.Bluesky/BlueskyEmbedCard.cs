using Newtonsoft.Json;

namespace X.Bluesky;

public class BlueskyEmbedCard
{
    public string Uri { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public BlueskyThumb Thumb { get; set; }
}

public record  BlueskyBlobResponse
{
    public BlueskyThumb Blob { get; set; }
}

public record BlueskyThumb
{
    [JsonProperty("$type")]
    public string Type { get; set; }

    [JsonProperty("ref")]
    public BlueskyThumbRef Ref { get; set; }

    [JsonProperty("mimeType")]
    public string MimeType { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }
}

public class BlueskyThumbRef
{
    [JsonProperty("$link")]
    public string Link { get; set; }
}
