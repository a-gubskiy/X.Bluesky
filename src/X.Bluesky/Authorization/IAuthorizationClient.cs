using X.Bluesky.Models;

namespace X.Bluesky.Authorization;

public interface IAuthorizationClient
{
    Task<Session> GetSession();
}