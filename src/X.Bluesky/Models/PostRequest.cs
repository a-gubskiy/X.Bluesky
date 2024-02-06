namespace X.Bluesky.Models;

public record PostRequest
{
    public string Repo { get; set; }= "";
    public string Collection { get; set; } = "";
    public Post Record { get; set; } = new();
}