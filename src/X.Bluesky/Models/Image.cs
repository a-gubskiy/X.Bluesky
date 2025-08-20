namespace X.Bluesky.Models;

/// <summary>
/// Represents an image that can be attached to a Bluesky post.
/// Contains the image data, type information, and accessibility text.
/// </summary>
public record Image
{
    /// <summary>
    /// Gets or initializes the binary content of the image.
    /// </summary>
    /// <value>A byte array containing the image data. Defaults to an empty array.</value>
    public byte[] Content { get; init; } = [];

    /// <summary>
    /// Gets or initializes the MIME type of the image.
    /// Specifies the format of the image (e.g., "image/jpeg", "image/png").
    /// </summary>
    /// <value>A string containing the MIME type. Defaults to an empty string.</value>
    public string MimeType { get; init; } = "";

    /// <summary>
    /// Gets or initializes the alternative text for the image.
    /// Used for accessibility purposes to describe the image content.
    /// </summary>
    /// <value>A string containing the alt text. Defaults to an empty string.</value>
    public string Alt { get; init; } = "";

    /// <summary>
    /// Gets or initializes the width of the image in pixels.
    /// This property is optional and can be null if the width is not specified.
    /// If both Width and Height are specified, an Aspect Ratio hint will be provided to Bluesky.
    /// </summary>
    public int? Width { get; init; } = null;

    /// <summary>
    /// Gets or initializes the height of the image in pixels.
    /// This property is optional and can be null if the height is not specified.
    /// If both Width and Height are specified, an Aspect Ratio hint will be provided to Bluesky.
    /// </summary>
    public int? Height { get; init; } = null;
}
