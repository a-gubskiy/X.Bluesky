using System.Collections.Immutable;

namespace X.Bluesky.Models.API;

internal record Facet
{
    public FacetIndex Index { get; set; } = new();

    public IReadOnlyCollection<FacetFeature> Features { get; set; } = ImmutableList<FacetFeature>.Empty;
}