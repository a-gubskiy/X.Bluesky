using X.Bluesky.Models;

namespace X.Bluesky.Authorization;

internal class ReusableAuthorizationClient : IAuthorizationClient
{
    private readonly IAuthorizationClient _authorizationClient;

    private Session? _session;
    private DateTime? _sessionRefreshedAt;

    public ReusableAuthorizationClient(IAuthorizationClient authorizationClient)
    {
        _authorizationClient = authorizationClient;
    }

    public async Task<Session> GetSession()
    {
        if (_session != null
            && _sessionRefreshedAt != null
            && _sessionRefreshedAt.Value.AddMinutes(90) > DateTime.UtcNow)
        {
            // Reuse existing session
            return _session;
        }

        _session = await _authorizationClient.GetSession();

        _sessionRefreshedAt = DateTime.UtcNow;

        return _session;
    }
}