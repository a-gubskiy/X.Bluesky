using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using X.Bluesky.Models;

namespace X.Bluesky.Authorization;

/// <summary>
/// Client for authenticating against the Bluesky AT Protocol and obtaining a session token.
/// </summary>
/// <remarks>
/// Uses an <see cref="IHttpClientFactory"/> to call the <c>com.atproto.server.createSession</c> endpoint
/// at the provided service base URI.
/// </remarks>
public class AuthorizationClient : IAuthorizationClient
{
    /// <summary>
    /// Factory used to create HTTP clients for outbound requests.
    /// </summary>
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Account identifier (handle, DID, or email) used for authentication.
    /// </summary>
    private readonly string _identifier;

    /// <summary>
    /// App password or account password corresponding to the identifier.
    /// </summary>
    private readonly string _password;

    /// <summary>
    /// Base service URI of the Bluesky/ATProto server.
    /// </summary>
    private readonly Uri _baseUri;

    /// <summary>
    /// Initializes a client targeting the default Bluesky service (<c>https://bsky.social</c>).
    /// </summary>
    /// <param name="identifier">Handle, DID, or email used to authenticate.</param>
    /// <param name="password">App password or account password associated with the identifier.</param>
    [PublicAPI]
    public AuthorizationClient(string identifier, string password)
        : this(new BlueskyHttpClientFactory(), identifier, password, new Uri("https://bsky.social"))
    {
    }

    /// <summary>
    /// Initializes a client targeting a specific Bluesky/ATProto service base URI.
    /// </summary>
    /// <param name="identifier">Handle, DID, or email used to authenticate.</param>
    /// <param name="password">App password or account password associated with the identifier.</param>
    /// <param name="baseUri">Service base URI (e.g., <c>https://bsky.social</c>).</param>
    [PublicAPI]
    public AuthorizationClient(string identifier, string password, Uri baseUri)
        : this(new BlueskyHttpClientFactory(), identifier, password, baseUri)
    {
    }

    /// <summary>
    /// Initializes a client with explicit dependencies.
    /// </summary>
    /// <param name="httpClientFactory">Factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="identifier">Handle, DID, or email used to authenticate.</param>
    /// <param name="password">App password or account password associated with the identifier.</param>
    /// <param name="baseUri">Service base URI (e.g., <c>https://bsky.social</c>).</param>
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

    /// <summary>
    /// Requests a new session for the configured identity by posting credentials to the
    /// <c>com.atproto.server.createSession</c> endpoint.
    /// </summary>
    /// <returns>A task that resolves to the created <see cref="Session"/>.</returns>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails or a non‑success status code is returned.</exception>
    /// <exception cref="JsonException">Thrown if the response payload cannot be deserialized into <see cref="Session"/>.</exception>
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