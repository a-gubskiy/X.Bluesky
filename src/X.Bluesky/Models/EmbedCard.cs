namespace X.Bluesky.Models;

public record EmbedCard
{
    public string Uri { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public Thumb? Thumb { get; set; }
}