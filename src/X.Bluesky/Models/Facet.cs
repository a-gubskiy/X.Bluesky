namespace X.Bluesky.Models;

public record Facet
{
    public FacetIndex Index { get; set; } = new();

    public IReadOnlyCollection<FacetFeature> Features { get; set; } = new List<FacetFeature>();
}
