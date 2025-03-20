using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using X.Bluesky.EmbedCards;
using X.Bluesky.Models;

namespace X.Bluesky;

[PublicAPI]
public interface IBlueskyClient
{
    /// <summary>
    /// Create post
    /// </summary>
    /// <param name="text">
    /// Post text
    /// </param>
    /// <returns></returns>
    Task Post(string text);

    /// <summary>
    /// Create post with link
    /// </summary>
    /// <param name="text">
    /// Post text
    /// </param>
    /// <param name="url">
    /// Url of attachment page
    /// </param>
    /// <param name="autoGenerateCard"></param>
    /// <returns></returns>
    Task Post(string text, Uri url, bool autoGenerateCard = true);

    /// <summary>
    /// Create post with image
    /// </summary>
    /// <param name="text"></param>
    /// <param name="image"></param>
    /// <returns></returns>
    Task Post(string text, Image image);

    /// <summary>
    /// Create post with link and image
    /// </summary>
    /// <param name="text"></param>
    /// <param name="url"></param>
    /// <param name="image"></param>
    /// <returns></returns>
    Task Post(string text, Uri? url, Image image);

    /// <summary>
    /// Create post with link and images
    /// </summary>
    /// <param name="text"></param>
    /// <param name="url"></param>
    /// <param name="images"></param>
    /// <returns></returns>
    Task Post(string text, Uri? url, IEnumerable<Image> images);
}

public class BlueskyClient : IBlueskyClient
{
    private readonly ILogger _logger;
    private readonly IAuthorizationClient _authorizationClient;
    private readonly IMentionResolver _mentionResolver;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _baseUrl;
    private readonly IReadOnlyCollection<string> _languages;

    /// <summary>
    /// Creates a new instance of the Bluesky client
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="identifier">Bluesky identifier</param>
    /// <param name="password">Bluesky application password</param>
    /// <param name="languages">Post languages</param>
    /// <param name="reuseSession">Reuse session</param>
    /// <param name="logger"></param>
    public BlueskyClient(
        IHttpClientFactory httpClientFactory,
        string identifier,
        string password,
        IEnumerable<string> languages,
        bool reuseSession,
        ILogger<BlueskyClient> logger)
        : this(httpClientFactory, identifier, password, languages, reuseSession, new Uri("https://bsky.social"), logger)
    {
    }

    /// <summary>
    /// Creates a new instance of the Bluesky client
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="languages">Post languages</param>
    /// <param name="baseUrl">Bluesky base url</param>
    /// <param name="logger"></param>
    /// <param name="mentionResolver"></param>
    /// <param name="authorizationClient"></param>
    public BlueskyClient(
        IHttpClientFactory httpClientFactory,
        IEnumerable<string> languages,
        Uri baseUrl,
        IMentionResolver mentionResolver,
        IAuthorizationClient authorizationClient,
        ILogger<BlueskyClient> logger)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _baseUrl = baseUrl;
        _languages = languages.ToFrozenSet();
        _mentionResolver = mentionResolver;
        _authorizationClient = authorizationClient;
    }

    /// <summary>
    /// Creates a new instance of the Bluesky client
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="identifier">User identifier</param>
    /// <param name="password">User password or application password</param>
    /// <param name="languages">Post languages</param>
    /// <param name="reuseSession">Indicates whether to reuse the session</param>
    /// <param name="baseUrl">Bluesky base url</param>>
    /// <param name="logger">Logger</param>
    public BlueskyClient(
        IHttpClientFactory httpClientFactory,
        string identifier,
        string password,
        IEnumerable<string> languages,
        bool reuseSession,
        Uri baseUrl,
        ILogger<BlueskyClient> logger)
        : this(
            httpClientFactory,
            languages,
            baseUrl,
            new MentionResolver(httpClientFactory, baseUrl, logger),
            new AuthorizationClient(httpClientFactory, identifier, password, reuseSession, baseUrl), logger)
    {
    }

    /// <summary>
    /// Creates a new instance of the Bluesky client
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="identifier">Bluesky identifier</param>
    /// <param name="password">Bluesky application password</param>
    public BlueskyClient(
        IHttpClientFactory httpClientFactory,
        string identifier,
        string password)
        : this(httpClientFactory, identifier, password, ["en", "en-US"], false, NullLogger<BlueskyClient>.Instance)
    {
    }

    /// <summary>
    /// Creates a new instance of the Bluesky client
    /// </summary>
    /// <param name="identifier">Bluesky identifier</param>
    /// <param name="password">Bluesky application password</param>
    /// <param name="reuseSession">Reuse session</param>
    /// <param name="logger"></param>
    public BlueskyClient(string identifier, string password, bool reuseSession, ILogger<BlueskyClient> logger)
        : this(new BlueskyHttpClientFactory(), identifier, password, ["en", "en-US"], reuseSession, logger)
    {
    }

    /// <summary>
    /// Creates a new instance of the Bluesky client
    /// </summary>
    /// <param name="identifier">Bluesky identifier</param>
    /// <param name="password">Bluesky application password</param>
    public BlueskyClient(string identifier, string password)
        : this(identifier, password, false, NullLogger<BlueskyClient>.Instance)
    {
    }

    /// <inheritdoc />
    public Task Post(string text) =>
        Post(text, null, ImmutableList<Image>.Empty);

    /// <inheritdoc />
    public Task Post(string text, Uri url, bool autoGenerateCard = true) =>
        Post(text, url, ImmutableList<Image>.Empty, autoGenerateCard);

    /// <inheritdoc />
    public Task Post(string text, Image image) =>
        Post(text, null, [image], false);

    /// <inheritdoc />
    public Task Post(string text, Uri? url, Image image) =>
        Post(text, url, [image], true);

    /// <inheritdoc />
    public Task Post(string text, Uri? url, IEnumerable<Image> images) =>
        Post(text, url, images.ToList(), true);

    private async Task Post(string text, Uri? url, IReadOnlyCollection<Image> images, bool generateCardForUrl = true)
    {
        var session = await _authorizationClient.GetSession();

        if (session == null)
        {
            throw new AuthenticationException("Unable to get session");
        }

        // Fetch the current time in ISO 8601 format, with "Z" to denote UTC
        var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        var facets = await ExtractFacets(text);

        // Required fields for the post
        var post = new Post
        {
            Type = "app.bsky.feed.post",
            Text = text,
            CreatedAt = now,
            Langs = _languages.ToList(),
            Facets = facets.ToList()
        };
        
        if (images.Any())
        {
            var embedBuilder = new EmbedImageBuilder(_httpClientFactory, session, _baseUrl, _logger);

            post.Embed = await embedBuilder.GetEmbedCard(images);
        }
        else
        {
            //If no image was defined we're trying to get link from facets
            if (url == null && generateCardForUrl)
            {
                //If no link was defined we're trying to get link from facets 
                url = ExtractUrlFromFacets(facets);
            }

            if (url != null)
            {
                var embedBuilder = new EmbedExternalBuilder(_httpClientFactory, session, _baseUrl, _logger);

                post.Embed = await embedBuilder.GetEmbedCard(url);
            }
        }

        var createPostRequest = new CreatePostRequest
        {
            Repo = session.Did,
            Collection = "app.bsky.feed.post",
            Record = post,
        };

        await CreatePost(createPostRequest, session);
    }

    /// <summary>
    /// Extracts facets from the given text and resolves any mentions found within those facets.
    /// </summary>
    /// <param name="text">The text to extract facets from.</param>
    /// <returns>A collection of facets extracted from the text with resolved mentions.</returns>
    /// <remarks>
    /// This method processes the text to identify facets (like mentions, links) and then
    /// specifically resolves mention DIDs by calling the mention resolver service.
    /// </remarks>
    private async Task<IReadOnlyCollection<Facet>> ExtractFacets(string text)
    {
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
    
        return facets;
    }

    /// <summary>
    /// Creates a new post by sending a request to the Bluesky API.
    /// </summary>
    /// <param name="createPostRequest">The request object containing the post details.</param>
    /// <param name="session">The current user session containing authentication details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP response status is an error code.</exception>
    private async Task CreatePost(CreatePostRequest createPostRequest, Session session)
    {
        var requestUri = $"{_baseUrl.ToString().TrimEnd('/')}/xrpc/com.atproto.repo.createRecord";
    
        var jsonRequest = JsonConvert.SerializeObject(createPostRequest, Formatting.Indented, new JsonSerializerSettings
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
    
            _logger.LogError("Error: {ResponseContent}", responseContent);
        }
    
        // This throws an exception if the HTTP response status is an error code.
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Extracts the first URL from a collection of facets by searching for FacetFeatureLink features.
    /// </summary>
    /// <param name="facets">The collection of facets to search through.</param>
    /// <returns>The first URL found within a FacetFeatureLink feature, or null if no URLs are found.</returns>
    private static Uri? ExtractUrlFromFacets(IReadOnlyCollection<Facet> facets)
    {
        var url = facets
            .SelectMany(facet => facet.Features)
            .Where(feature => feature is FacetFeatureLink)
            .Cast<FacetFeatureLink>()
            .Select(f => f.Uri)
            .FirstOrDefault();

        return url;
    }
}