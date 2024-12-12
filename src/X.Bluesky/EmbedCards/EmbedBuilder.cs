using X.Bluesky.Models;

namespace X.Bluesky.EmbedCards;

public interface IEmbedBuilder
{
    
}

public abstract class EmbedBuilder : IEmbedBuilder
{
    protected readonly Session _session;

    protected EmbedBuilder(Session session)
    {
        _session = session;
    }
}