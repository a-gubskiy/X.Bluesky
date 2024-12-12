using X.Bluesky.Models;

namespace X.Bluesky.EmbedCards;

public abstract class EmbedBuilder
{
    protected readonly Session Session;

    protected EmbedBuilder(Session session)
    {
        Session = session;
    }
}