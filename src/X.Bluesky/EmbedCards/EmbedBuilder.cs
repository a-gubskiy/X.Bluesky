using X.Bluesky.Models;
using X.Bluesky.Models.API;

namespace X.Bluesky.EmbedCards;

internal abstract class EmbedBuilder
{
    protected readonly Session Session;

    protected EmbedBuilder(Session session)
    {
        Session = session;
    }
}