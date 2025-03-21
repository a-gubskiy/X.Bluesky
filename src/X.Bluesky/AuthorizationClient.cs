using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using X.Bluesky.Models;
using X.Bluesky.Models.API;

namespace X.Bluesky;

public interface IAuthorizationClient
{
    Task<Session> GetSession();
}

public class AuthorizationClient : IAuthorizationClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _identifier;
    private readonly string _password;

    /// <summary>
    /// Session reuse flag
    /// </summary>
    private readonly bool _reuseSession;

    private readonly Uri _baseUri;

    private Session? _session;
    private DateTime? _sessionRefreshedAt;

    [PublicAPI]
    public AuthorizationClient(string identifier, string password)
        : this(new BlueskyHttpClientFactory(), identifier, password, false, new Uri("https://bsky.social"))
    {
    }

    [PublicAPI]
    public AuthorizationClient(string identifier, string password, bool reuseSession, Uri baseUri)
        : this(new BlueskyHttpClientFactory(), identifier, password, reuseSession, baseUri)
    {
    }

    [PublicAPI]
    public AuthorizationClient(
        IHttpClientFactory httpClientFactory,
        string identifier,
        string password,
        bool reuseSession,
        Uri baseUri)
    {
        _reuseSession = reuseSession;
        _baseUri = baseUri;
        _httpClientFactory = httpClientFactory;
        _identifier = identifier;
        _password = password;
    }

    /// <summary>
    /// Authorize in Bluesky
    /// </summary>
    /// <returns>
    /// Instance of authorized session
    /// </returns>
    public async Task<Session> GetSession()
    {
        if (_reuseSession && _session != null
                          && _sessionRefreshedAt != null
                          && _sessionRefreshedAt.Value.AddMinutes(90) > DateTime.UtcNow)
        {
            // Reuse existing session
            return _session;
        }

        var requestData = new
        {
            identifier = _identifier,
            password = _password
        };

        var json = JsonConvert.SerializeObject(requestData);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();

        var uri = $"{_baseUri.ToString().TrimEnd('/')}/xrpc/com.atproto.server.createSession";

        var response = await httpClient.PostAsync(uri, content);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        _session = JsonConvert.DeserializeObject<Session>(jsonResponse)!;
        _sessionRefreshedAt = DateTime.UtcNow;

        return _session;
    }
}