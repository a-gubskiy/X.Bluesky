namespace X.Bluesky.Models.API;

internal record CreatePostRequest
{
    public string Repo { get; set; }= "";
    
    public string Collection { get; set; } = "";
    
    public Post Record { get; set; } = new();
}