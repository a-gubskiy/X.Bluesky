namespace X.Bluesky.Models;

public record Session
{
    public string AccessJwt { get; set; } = "";
    public string Did { get; set; } = "";
}