namespace X.Bluesky.Models;

public record CreatePostRequest
{
    public string Repo { get; set; }= "";
    public string Collection { get; set; } = "";
    public Post Record { get; set; } = new();
}