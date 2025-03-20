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

/// <summary>
/// Provides methods to interact with the Bluesky social network API.
/// </summary>
[PublicAPI]
public interface IBlueskyClient
{
    /// <summary>
    /// Creates a post on Bluesky with text content only.
    /// </summary>
    /// <param name="text">The text content of the post.</param>
    /// <returns>A task representing the asynchronous post operation.</returns>
    Task Post(string text);

    /// <summary>
    /// Creates a post on Bluesky with text content and a URL.
    /// </summary>
    /// <param name="text">The text content of the post.</param>
    /// <param name="url">The URL to include in the post.</param>
    /// <param name="autoGenerateCard">Whether to automatically generate a preview card for the URL. Default is true.</param>
    /// <returns>A task representing the asynchronous post operation.</returns>
    Task Post(string text, Uri url, bool autoGenerateCard = true);

    /// <summary>
    /// Creates a post on Bluesky with text content and a single image.
    /// </summary>
    /// <param name="text">The text content of the post.</param>
    /// <param name="image">The image to include in the post.</param>
    /// <returns>A task representing the asynchronous post operation.</returns>
    Task Post(string text, Image image);

    /// <summary>
    /// Creates a post on Bluesky with text content, a URL, and a single image.
    /// </summary>
    /// <param name="text">The text content of the post.</param>
    /// <param name="url">The optional URL to include in the post.</param>
    /// <param name="image">The image to include in the post.</param>
    /// <returns>A task representing the asynchronous post operation.</returns>
    Task Post(string text, Uri? url, Image image);

    /// <summary>
    /// Creates a post on Bluesky with text content, a URL, and multiple images.
    /// </summary>
    /// <param name="text">The text content of the post.</param>
    /// <param name="url">The optional URL to include in the post.</param>
    /// <param name="images">The collection of images to include in the post.</param>
    /// <returns>A task representing the asynchronous post operation.</returns>
    Task Post(string text, Uri? url, IEnumerable<Image> images);
}

/// <summary>
/// A client for interacting with the Bluesky social network API.
/// </summary>
/// <remarks>
/// This class provides implementation for the IBlueskyClient interface.
/// It offers methods to authenticate with Bluesky and to create various types of posts,
/// including text-only posts, posts with URLs, and posts with images.
/// The client handles authentication, content posting, mention resolution,
/// and embed card generation for rich media content.
/// </remarks>
public class BlueskyClient : IBlueskyClient
{
    private readonly ILogger _logger;
    private readonly IAuthorizationClient _authorizationClient;
    private readonly IMentionResolver _mentionResolver;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _baseUrl;
    private readonly IReadOnlyCollection<string> _languages;


    /// <summary>
    /// Initializes a new instance of the <see cref="BlueskyClient"/> class with the default Bluesky API base URL.
    /// </summary>
    /// <param name="httpClientFactory">The factory for creating HTTP clients.</param>
    /// <param name="identifier">The user identifier (handle or email) for authentication.</param>
    /// <param name="password">The user password for authentication.</param>
    /// <param name="languages">The list of languages supported for content.</param>
    /// <param name="reuseSession">Whether to reuse authentication session between requests.</param>
    /// <param name="logger">The logger instance.</param>
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
    /// Initializes a new instance of the <see cref="BlueskyClient"/> class with explicit dependency injection.
    /// </summary>
    /// <param name="httpClientFactory">The factory for creating HTTP clients.</param>
    /// <param name="languages">The list of languages supported for content.</param>
    /// <param name="baseUrl">The base URL of the Bluesky API.</param>
    /// <param name="mentionResolver">The service for resolving mentions in posts.</param>
    /// <param name="authorizationClient">The service for handling authentication.</param>
    /// <param name="logger">The logger instance.</param>
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
    /// Initializes a new instance of the <see cref="BlueskyClient"/> class with a custom base URL.
    /// </summary>
    /// <param name="httpClientFactory">The factory for creating HTTP clients.</param>
    /// <param name="identifier">The user identifier (handle or email) for authentication.</param>
    /// <param name="password">The user password for authentication.</param>
    /// <param name="languages">The list of languages supported for content.</param>
    /// <param name="reuseSession">Whether to reuse authentication session between requests.</param>
    /// <param name="baseUrl">The base URL of the Bluesky API.</param>
    /// <param name="logger">The logger instance.</param>
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
    /// Initializes a new instance of the <see cref="BlueskyClient"/> class with default settings.
    /// </summary>
    /// <param name="httpClientFactory">The factory for creating HTTP clients.</param>
    /// <param name="identifier">The user identifier (handle or email) for authentication.</param>
    /// <param name="password">The user password for authentication.</param>
    public BlueskyClient(
        IHttpClientFactory httpClientFactory,
        string identifier,
        string password)
        : this(httpClientFactory, identifier, password, ["en", "en-US"], false, NullLogger<BlueskyClient>.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueskyClient"/> class with a custom HTTP client factory.
    /// </summary>
    /// <param name="identifier">The user identifier (handle or email) for authentication.</param>
    /// <param name="password">The user password for authentication.</param>
    /// <param name="reuseSession">Whether to reuse authentication session between requests.</param>
    /// <param name="logger">The logger instance.</param>
    public BlueskyClient(string identifier, string password, bool reuseSession, ILogger<BlueskyClient> logger)
        : this(new BlueskyHttpClientFactory(), identifier, password, ["en", "en-US"], reuseSession, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueskyClient"/> class with minimal configuration.
    /// </summary>
    /// <param name="identifier">The user identifier (handle or email) for authentication.</param>
    /// <param name="password">The user password for authentication.</param>
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

        var facets = await ExtractFacets(text);
        var embedCard = await GetEmbedCard(url, images, facets, generateCardForUrl);

        // Required fields for the post
        var post = new Post
        {
            Type = "app.bsky.feed.post",
            Text = text,
            CreatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            Langs = _languages.ToList(),
            Facets = facets.ToList(),
            Embed = embedCard
        };

        var createPostRequest = new CreatePostRequest
        {
            Repo = session.Did,
            Collection = "app.bsky.feed.post",
            Record = post,
        };

        await CreatePost(createPostRequest);
    }

    /// <summary>
    /// Determines and creates the appropriate embed card based on the provided parameters.
    /// </summary>
    /// <param name="url">Optional URL to create an external embed card for.</param>
    /// <param name="images">Collection of images to create an image embed card from.</param>
    /// <param name="facets">Collection of facets that may contain a URL if none is explicitly provided.</param>
    /// <param name="generateCardForUrl">Flag indicating whether to generate a card for the URL.</param>
    /// <returns>
    /// An embed card appropriate for the provided content:
    /// - Image embed if images are provided
    /// - External embed if a URL is available and generation is enabled
    /// - Null if no embeddable content is available or generation is disabled
    /// </returns>
    /// <remarks>
    /// This method follows a priority order: images take precedence over URLs. 
    /// If no URL is explicitly provided, it will attempt to extract one from facets.
    /// </remarks>
    private async Task<IEmbed?> GetEmbedCard(
        Uri? url, IReadOnlyCollection<Image> images,
        IReadOnlyCollection<Facet> facets,
        bool generateCardForUrl)
    {
        var session = await _authorizationClient.GetSession();

        if (images.Any())
        {
            var embedImageBuilder = new EmbedImageBuilder(_httpClientFactory, session, _baseUrl, _logger);

            return await embedImageBuilder.GetEmbedCard(images);
        }

        if (!generateCardForUrl)
        {
            return null;
        }

        //If no image was defined we're trying to get link from facets
        if (url == null)
        {
            //If no link was defined we're trying to get link from facets 
            url = ExtractUrlFromFacets(facets);
        }

        if (url == null)
        {
            return null;
        }

        var embedExternalBuilder = new EmbedExternalBuilder(_httpClientFactory, session, _baseUrl, _logger);

        return await embedExternalBuilder.GetEmbedCard(url);
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
                if (facetFeature is not FacetFeatureMention facetFeatureMention)
                {
                    continue;
                }

                var resolveDid = await _mentionResolver.ResolveMention(facetFeatureMention.Did);

                facetFeatureMention.ResolveDid(resolveDid);
            }
        }

        return facets;
    }

    /// <summary>
    /// Creates a new post by sending a request to the Bluesky API.
    /// </summary>
    /// <param name="createPostRequest">The request object containing the post details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP response status is an error code.</exception>
    private async Task CreatePost(CreatePostRequest createPostRequest)
    {
        var session = await _authorizationClient.GetSession();
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