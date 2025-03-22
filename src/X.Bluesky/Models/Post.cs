using System.Collections.Immutable;
using X.Bluesky.Models.API;

namespace X.Bluesky.Models;

/// <summary>
/// Represents a post to be published to the Bluesky social network.
/// Contains all content and metadata for creating a post.
/// </summary>
public record Post
{
    /// <summary>
    /// Gets or initializes the text content of the post.
    /// </summary>
    /// <value>The text content string. Defaults to an empty string.</value>
    public string Text { get; init; } = "";

    /// <summary>
    /// Gets or initializes the language codes that apply to this post.
    /// Used to identify the languages used in the post content.
    /// </summary>
    /// <value>A collection of language code strings. Defaults to an empty collection.</value>
    public IReadOnlyCollection<string> Languages { get; init; } = ImmutableList<string>.Empty;

    /// <summary>
    /// Gets or initializes an optional URL to include with the post.
    /// Can be used to link to external content.
    /// </summary>
    /// <value>The URL to include, or null if no URL is specified.</value>
    public Uri? Url { get; init; } = null;

    /// <summary>
    /// Gets or initializes the collection of images to attach to the post.
    /// </summary>
    /// <value>A collection of <see cref="Image"/> objects. Defaults to an empty collection.</value>
    public IReadOnlyCollection<Image> Images { get; init; } = ImmutableList<Image>.Empty;

    /// <summary>
    /// Gets or initializes a value indicating whether to generate a preview card for the URL.
    /// When true, a rich preview card will be generated for any URL in the post.
    /// </summary>
    /// <value>True to generate a card (default); otherwise, false.</value>
    public bool GenerateCardForUrl { get; init; } = true;
}