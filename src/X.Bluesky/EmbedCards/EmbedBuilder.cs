using X.Bluesky.Models;

namespace X.Bluesky.EmbedCards;

public interface IEmbedBuilder
{
    
}

public abstract class EmbedBuilder : IEmbedBuilder
{
    protected readonly Session Session;

    protected EmbedBuilder(Session session)
    {
        Session = session;
    }
}