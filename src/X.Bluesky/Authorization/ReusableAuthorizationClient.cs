using X.Bluesky.Models;

namespace X.Bluesky.Authorization;

/// <summary>
/// Authorization client wrapper that caches and reuses a session for up to 90 minutes,
/// reducing repeated calls to the underlying <see cref="IAuthorizationClient"/>.
/// </summary>
/// <remarks>
/// A session is considered fresh if it was refreshed less than 90 minutes ago (based on <see cref="DateTime.UtcNow"/>).
/// This implementation is not thread-safe; concurrent callers may race when refreshing the session.
/// </remarks>
internal class ReusableAuthorizationClient : IAuthorizationClient
{
    // The underlying client used to obtain sessions.
    private readonly IAuthorizationClient _authorizationClient;

    // Cached session instance.
    private Session? _session;

    // UTC timestamp of the last session refresh.
    private DateTime? _sessionRefreshedAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReusableAuthorizationClient"/> class.
    /// </summary>
    /// <param name="authorizationClient">
    /// The underlying authorization client used to fetch sessions when the cache is stale or empty.
    /// </param>
    public ReusableAuthorizationClient(IAuthorizationClient authorizationClient)
    {
        _authorizationClient = authorizationClient;
    }

    /// <summary>
    /// Gets a session, reusing a cached one if it is still fresh; otherwise fetches and caches a new session.
    /// </summary>
    /// <returns>
    /// An awaitable task producing a <see cref="Session"/> instance.
    /// </returns>
    /// <remarks>
    /// The cached session is reused if it was refreshed within the last 90 minutes.
    /// </remarks>
    /// <exception cref="System.Exception">
    /// Propagated from the underlying <see cref="IAuthorizationClient.GetSession"/> when retrieval fails.
    /// </exception>
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