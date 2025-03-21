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
using X.Bluesky.Models.API;

namespace X.Bluesky;

/// <summary>
/// Provides methods to interact with the Bluesky social network API.
/// </summary>
[PublicAPI]
public interface IBlueskyClient
{
    /// <summary>
    /// Posts content to the Bluesky social network.
    /// </summary>
    /// <param name="post">The post object containing all the content and metadata to be published.</param>
    /// <returns>A task representing the asynchronous operation of posting to Bluesky.</returns>
    /// <exception cref="System.Security.Authentication.AuthenticationException">Thrown when unable to obtain a valid session.</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when the API request fails.</exception>
    Task Post(Models.Post post);
    
    Task Post(string text) => Post(new Models.Post { Text = text });
    
    Task Post(string text, Uri uri) => Post(new Models.Post { Text = text, Url = uri });

    Task Post(string text, Image image) => Post(new Models.Post { Text = text, Images = [image] });

    Task Post(string text, Uri uri, Image image) => Post(new Models.Post { Text = text, Url = uri, Images = [image] });
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
    private readonly Uri _baseUrl;
    private readonly ILogger _logger;
    private readonly IMentionResolver _mentionResolver;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthorizationClient _authorizationClient;

    private static Uri DefaultBaseUrl => new Uri("https://bsky.social");

    public BlueskyClient(
        IAuthorizationClient authorizationClient,
        Uri? baseUrl = null,
        IHttpClientFactory? httpClientFactory = null,
        IMentionResolver? mentionResolver = null,
        ILogger<BlueskyClient>? logger = null)
    {
        _logger = logger ?? new NullLogger<BlueskyClient>();
        _baseUrl = baseUrl ?? DefaultBaseUrl;
        _authorizationClient = authorizationClient;
        _httpClientFactory = httpClientFactory ?? new BlueskyHttpClientFactory();
        _mentionResolver = mentionResolver ?? new MentionResolver(_httpClientFactory, _baseUrl, _logger);
    }

    public BlueskyClient(
        string identifier,
        string password,
        Uri? baseUrl = null,
        IHttpClientFactory? httpClientFactory = null,
        IMentionResolver? mentionResolver = null,
        ILogger<BlueskyClient>? logger = null)
        : this(
            new AuthorizationClient(identifier, password, true, baseUrl ?? DefaultBaseUrl),
            baseUrl,
            httpClientFactory,
            mentionResolver, logger)
    {
    }

    /// <inheritdoc />
    public async Task Post(Models.Post post)
    {
        var session = await _authorizationClient.GetSession();

        if (session == null)
        {
            throw new AuthenticationException("Unable to get session");
        }

        var facets = await ExtractFacets(post.Text);
        var embedCard = await GetEmbedCard(post.Url, post.Images, facets, post.GenerateCardForUrl);

        var createPostRequest = new CreatePostRequest
        {
            Repo = session.Did,
            Collection = "app.bsky.feed.post",
            Record = new Post
            {
                Type = "app.bsky.feed.post",
                Text = post.Text,
                CreatedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                Langs = post.Languages.ToList(),
                Facets = facets.ToList(),
                Embed = embedCard
            },
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