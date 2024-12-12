using Newtonsoft.Json;

namespace X.Bluesky.Models;

public class EmbedImage : IEmbed
{
    [JsonProperty("$type")]
    public string Type => "app.bsky.embed.images";

    public List<ImageData> Images { get; set; } = new();
}

public record ImageData
{
    public string Alt { get; set; } = "";

    public Thumb Image { get; set; } = new();

    public AspectRatio AspectRatio { get; set; } = new();
}

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

public record AspectRatio
{
    public int Width { get; set; }

    public int Height { get; set; }
}