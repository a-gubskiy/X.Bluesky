namespace X.Bluesky;

public record BlueskyPostRequest
{
    public string Repo { get; set; }= "";
    public string Collection { get; set; } = "";
    public BlueskyPost Record { get; set; } = new();
}