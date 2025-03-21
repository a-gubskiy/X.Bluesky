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
/// Defines the core functionality for a Bluesky client that can post content to the Bluesky social network.
/// </summary>
[PublicAPI]
public interface IBlueskyClient
{
    /// <summary>
    /// Posts content to the Bluesky social network.
    /// </summary>
    /// <param name="post">The post object containing all the content and metadata to be published.</param>
    /// <returns>A task representing the asynchronous operation of posting to Bluesky.</returns>
    Task Post(Models.Post post);

    /// <summary>
    /// Posts a text-only message to the Bluesky social network.
    /// </summary>
    /// <param name="text">The text content to post.</param>
    /// <returns>A task representing the asynchronous operation of posting to Bluesky.</returns>
    Task Post(string text) => Post(new Models.Post { Text = text });

    /// <summary>
    /// Posts a message with a URL to the Bluesky social network.
    /// </summary>
    /// <param name="text">The text content to post.</param>
    /// <param name="uri">The URL to include in the post.</param>
    /// <returns>A task representing the asynchronous operation of posting to Bluesky.</returns>
    Task Post(string text, Uri uri) => Post(new Models.Post { Text = text, Url = uri });

    /// <summary>
    /// Posts a message with an image to the Bluesky social network.
    /// </summary>
    /// <param name="text">The text content to post.</param>
    /// <param name="image">The image to include in the post.</param>
    /// <returns>A task representing the asynchronous operation of posting to Bluesky.</returns>
    Task Post(string text, Image image) => Post(new Models.Post { Text = text, Images = [image] });

    /// <summary>
    /// Posts a message with both a URL and an image to the Bluesky social network.
    /// </summary>
    /// <param name="text">The text content to post.</param>
    /// <param name="uri">The URL to include in the post.</param>
    /// <param name="image">The image to include in the post.</param>
    /// <returns>A task representing the asynchronous operation of posting to Bluesky.</returns>
    Task Post(string text, Uri uri, Image image) => Post(new Models.Post { Text = text, Url = uri, Images = [image] });
}

/// <summary>
/// Implementation of <see cref="IBlueskyClient"/> that provides functionality for posting content to the Bluesky social network.
/// </summary>
public class BlueskyClient : IBlueskyClient
{
    private readonly Uri _baseUrl;
    private readonly ILogger _logger;
    private readonly IMentionResolver _mentionResolver;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthorizationClient _authorizationClient;

    /// <summary>
    /// Gets the default Bluesky API base URL (https://bsky.social).
    /// </summary>
    private static Uri DefaultBaseUrl => new Uri("https://bsky.social");

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueskyClient"/> class using an authorization client.
    /// </summary>
    /// <param name="authorizationClient">The client used for handling authentication with Bluesky.</param>
    /// <param name="baseUrl">The base URL for the Bluesky API. If null, defaults to https://bsky.social.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients. If null, a default implementation is used.</param>
    /// <param name="logger">Logger for recording diagnostic information. If null, a null logger is used.</param>
    public BlueskyClient(
        IAuthorizationClient authorizationClient,
        Uri? baseUrl = null,
        IHttpClientFactory? httpClientFactory = null,
        ILogger<BlueskyClient>? logger = null)
    {
        _logger = logger ?? new NullLogger<BlueskyClient>();
        _baseUrl = baseUrl ?? DefaultBaseUrl;
        _authorizationClient = authorizationClient;
        _httpClientFactory = httpClientFactory ?? new BlueskyHttpClientFactory();
        _mentionResolver = new MentionResolver(_httpClientFactory, _baseUrl, _logger);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueskyClient"/> class using direct authentication credentials.
    /// </summary>
    /// <param name="identifier">The Bluesky user identifier (username or email).</param>
    /// <param name="password">The password for the Bluesky account.</param>
    /// <param name="baseUrl">The base URL for the Bluesky API. If null, defaults to https://bsky.social.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP clients. If null, a default implementation is used.</param>
    /// <param name="logger">Logger for recording diagnostic information. If null, a null logger is used.</param>
    public BlueskyClient(
        string identifier,
        string password,
        Uri? baseUrl = null,
        IHttpClientFactory? httpClientFactory = null,
        ILogger<BlueskyClient>? logger = null)
        : this(
            new AuthorizationClient(identifier, password, true, baseUrl ?? DefaultBaseUrl),
            baseUrl,
            httpClientFactory,
            logger)
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
    /// Builds an embed card for the post based on URLs and images.
    /// </summary>
    /// <param name="url">The explicit URL to create a card for, if any.</param>
    /// <param name="images">Collection of images to embed in the post.</param>
    /// <param name="facets">The facets extracted from the post text, which may contain URLs.</param>
    /// <param name="generateCardForUrl">Whether to generate a card for URLs found in the post.</param>
    /// <returns>An embed card object if one could be created; otherwise, null.</returns>
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
    /// Extracts facets (special features like mentions, links, and hashtags) from the post text.
    /// </summary>
    /// <param name="text">The text content to extract facets from.</param>
    /// <returns>A collection of facets with their positions and features.</returns>
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
    /// Sends a create post request to the Bluesky API.
    /// </summary>
    /// <param name="createPostRequest">The request object containing all post details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
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
    /// Extracts the first URL found in a collection of facets.
    /// </summary>
    /// <param name="facets">The collection of facets to search for URLs.</param>
    /// <returns>The first URL found, or null if none are present.</returns>
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