using System.Collections.Immutable;
using X.Bluesky.Models.API;

namespace X.Bluesky.Models;

public record Post
{
    public string Text { get; init; } = "";

    public IReadOnlyCollection<string> Languages { get; init; } = ImmutableList<string>.Empty;

    public Uri? Url { get; init; } = null;

    public IReadOnlyCollection<Image> Images { get; init; } = ImmutableList<Image>.Empty;

    public bool GenerateCardForUrl { get; init; } = true;
}