namespace X.Bluesky.Models;

public record Image
{
    /// <summary>
    /// Image content
    /// </summary>
    public byte[] Content { get; set; } = [];

    /// <summary>
    /// Image mime type
    /// </summary>
    public string MimeType { get; set; } = "";

    /// <summary>
    /// Image alt text
    /// </summary>
    public string Alt { get; set; } = "";
}