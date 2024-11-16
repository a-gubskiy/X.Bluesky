using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using X.Bluesky.Models;

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

    [PublicAPI]
    public AuthorizationClient(string identifier, string password)
        : this(new BlueskyHttpClientFactory(), identifier, password)
    {
    }

    [PublicAPI]
    public AuthorizationClient(IHttpClientFactory httpClientFactory, string identifier, string password)
    {
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
        var requestData = new
        {
            identifier = _identifier,
            password = _password
        };

        var json = JsonConvert.SerializeObject(requestData);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();

        var uri = "https://bsky.social/xrpc/com.atproto.server.createSession";
        var response = await httpClient.PostAsync(uri, content);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<Session>(jsonResponse)!;
    }
}