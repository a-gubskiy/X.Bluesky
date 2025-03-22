using Newtonsoft.Json;

namespace X.Bluesky.Models.API;

internal class EmbedImage : IEmbed
{
    [JsonProperty("$type")]
    public string Type => "app.bsky.embed.images";

    public List<ImageData> Images { get; set; } = new();
}

internal record ImageData
{
    public string Alt { get; set; } = "";

    public Thumb Image { get; set; } = new();

    public AspectRatio? AspectRatio { get; set; } = null;
}

internal record Thumb
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

internal class ThumbRef
{
    [JsonProperty("$link")]
    public string Link { get; set; } = "";
}

internal record AspectRatio
{
    public int Width { get; set; }

    public int Height { get; set; }
}