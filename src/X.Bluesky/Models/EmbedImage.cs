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

    public Image Image { get; set; } = new();

    public AspectRatio AspectRatio { get; set; } = new();
}

public record Image
{
    public string Type { get; set; } = "blob";

    public Ref Ref { get; set; } = new();

    public string MimeType { get; set; } = "image/png";

    public int Size { get; set; }
}

public record Ref
{
    public string Link { get; set; } = "";
}

public record AspectRatio
{
    public int Width { get; set; }

    public int Height { get; set; }
}