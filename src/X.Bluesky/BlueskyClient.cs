using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using X.Bluesky.Models;

namespace X.Bluesky;

[PublicAPI]
public interface IBlueskyClient
{
    /// <summary>
    /// Make post with link
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    Task Post(string text);
}

public class BlueskyClient : IBlueskyClient
{
    private readonly ILogger _logger;
    private readonly IAuthorizationClient _authorizationClient;
    private readonly IMentionResolver _mentionResolver;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IReadOnlyCollection<string> _languages;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="identifier">Bluesky identifier</param>
    /// <param name="password">Bluesky application password</param>
    /// <param name="languages">Post languages</param>
    /// <param name="logger"></param>
    public BlueskyClient(
        IHttpClientFactory httpClientFactory,
        string identifier,
        string password,
        IEnumerable<string> languages,
        ILogger<BlueskyClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _authorizationClient = new AuthorizationClient(httpClientFactory, identifier, password);
        _logger = logger;
        _languages = languages.ToImmutableList();
        _mentionResolver = new MentionResolver(_httpClientFactory);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="identifier">Bluesky identifier</param>
    /// <param name="password">Bluesky application password</param>
    public BlueskyClient(
        IHttpClientFactory httpClientFactory,
        string identifier,
        string password)
        : this(httpClientFactory, identifier, password, new[] { "en", "en-US" }, NullLogger<BlueskyClient>.Instance)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier">Bluesky identifier</param>
    /// <param name="password">Bluesky application password</param>
    public BlueskyClient(string identifier, string password)
        : this(new BlueskyHttpClientFactory(), identifier, password)
    {
    }
    
    /// <summary>
    /// Create post
    /// </summary>
    /// <param name="text">Post text</param>
    /// <returns></returns>
    public async Task Post(string text)
    {
        var session = await _authorizationClient.GetSession();

        if (session == null)
        {
            throw new AuthenticationException();
        }

        // Fetch the current time in ISO 8601 format, with "Z" to denote UTC
        var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var facetBuilder = new FacetBuilder();

        var facets = facetBuilder.GetFacets(text);

        foreach (var facet in facets)
        {
            foreach (var facetFeature in facet.Features)
            {
                if (facetFeature is FacetFeatureMention facetFeatureMention)
                {
                    var resolveDid = await _mentionResolver.ResolveMention(facetFeatureMention.Did);

                    facetFeatureMention.ResolveDid(resolveDid);
                }
            }
        }

        // Required fields for the post
        var post = new Post
        {
            Type = "app.bsky.feed.post",
            Text = text,
            CreatedAt = now,
            Langs = _languages.ToList(),
            Facets = facets.ToList()
        };


        //If no link was defined we're trying to get link from facets 
        var url = facets
            .SelectMany(facet => facet.Features)
            .Where(feature => feature is FacetFeatureLink)
            .Cast<FacetFeatureLink>()
            .Select(f => f.Uri)
            .FirstOrDefault();

        if (url != null)
        {
            var embedCardBuilder = new EmbedCardBuilder(_httpClientFactory, session, _logger);

            post.Embed = new Embed
            {
                External = await embedCardBuilder.Create(url),
                Type = "app.bsky.embed.external"
            };
        }

        var requestUri = "https://bsky.social/xrpc/com.atproto.repo.createRecord";

        var requestData = new CreatePostRequest
        {
            Repo = session.Did,
            Collection = "app.bsky.feed.post",
            Record = post,
        };

        var jsonRequest = JsonConvert.SerializeObject(requestData, Formatting.Indented, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        });

        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();

        // Add the Authorization header with the bearer token
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessJwt);

        var response = await httpClient.PostAsync(requestUri, content);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogError(responseContent);
        }

        // This throws an exception if the HTTP response status is an error code.
        response.EnsureSuccessStatusCode();
    }
}