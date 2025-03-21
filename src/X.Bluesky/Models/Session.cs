namespace X.Bluesky.Models;

public record Session
{
    public string AccessJwt { get; init; } = "";

    public string Did { get; init; } = "";
}