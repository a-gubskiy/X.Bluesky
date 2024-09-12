namespace X.Bluesky.Models;

public record Facet
{
    public FacetIndex Index { get; set; } = new();

    public List<FacetFeature> Features { get; set; } = new();
}
