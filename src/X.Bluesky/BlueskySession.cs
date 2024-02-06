namespace X.Bluesky;

public record BlueskySession
{
    public string AccessJwt { get; set; } = "";
    public string Did { get; set; } = "";
}