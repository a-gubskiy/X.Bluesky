namespace X.Bluesky.Models;

/// <summary>
/// Bluesky session
/// </summary>
public record Session
{
    public string AccessJwt { get; set; } = "";
    
    public string Did { get; set; } = "";
}