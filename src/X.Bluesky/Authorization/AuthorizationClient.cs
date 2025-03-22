using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using X.Bluesky.Models;

namespace X.Bluesky.Authorization;

public class AuthorizationClient : IAuthorizationClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _identifier;
    private readonly string _password;

    /// <summary>
    /// Session reuse flag
    /// </summary>
    // private readonly bool _reuseSession;
    private readonly Uri _baseUri;

    [PublicAPI]
    public AuthorizationClient(string identifier, string password)
        : this(new BlueskyHttpClientFactory(), identifier, password, new Uri("https://bsky.social"))
    {
    }

    [PublicAPI]
    public AuthorizationClient(string identifier, string password, Uri baseUri)
        : this(new BlueskyHttpClientFactory(), identifier, password, baseUri)
    {
    }

    [PublicAPI]
    public AuthorizationClient(
        IHttpClientFactory httpClientFactory,
        string identifier,
        string password,
        Uri baseUri)
    {
        _baseUri = baseUri;
        _httpClientFactory = httpClientFactory;
        _identifier = identifier;
        _password = password;
    }

    public async Task<Session> GetSession()
    {
        var requestData = new
        {
            identifier = _identifier,
            password = _password
        };

        const string mediaType = "application/json";
        
        var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, mediaType);

        var httpClient = _httpClientFactory.CreateClient();

        var uri = $"{_baseUri.ToString().TrimEnd('/')}/xrpc/com.atproto.server.createSession";

        var response = await httpClient.PostAsync(uri, content);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var session = JsonConvert.DeserializeObject<Session>(json)!;

        return session;
    }
}