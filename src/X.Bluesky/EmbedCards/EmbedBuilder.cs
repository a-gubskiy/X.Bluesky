using X.Bluesky.Models;

namespace X.Bluesky.EmbedCards;

/// <summary>
/// Base abstract class for builders that create embedded content for Bluesky posts.
/// Provides common functionality for different types of embed content builders.
/// </summary>
/// <remarks>
/// This class serves as the foundation for specialized embed builders like image embeds
/// or external link embeds used in Bluesky posts.
/// </remarks>
internal abstract class EmbedBuilder
{
    /// <summary>
    /// Gets the authentication session used for API requests when building embeds.
    /// </summary>
    protected readonly Session Session;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbedBuilder"/> class.
    /// </summary>
    /// <param name="session">The authentication session to use for API requests.</param>
    protected EmbedBuilder(Session session)
    {
        Session = session;
    }
}