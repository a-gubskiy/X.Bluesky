namespace X.Bluesky.Models.API;

public record Image
{
    /// <summary>
    /// Image content
    /// </summary>
    public byte[] Content { get; init; } = [];

    /// <summary>
    /// Image mime type
    /// </summary>
    public string MimeType { get; init; } = "";

    /// <summary>
    /// Image alt text
    /// </summary>
    public string Alt { get; init; } = "";
}