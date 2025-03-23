using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace X.Bluesky;

/// <summary>
/// Interface for resolving Bluesky user mentions to their decentralized identifiers (DIDs).
/// </summary>
public interface IMentionResolver
{
    /// <summary>
    /// Resolves a Bluesky handle or mention to its decentralized identifier (DID).
    /// </summary>
    /// <param name="mention">The handle or mention to resolve (with or without '@' prefix).</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the resolved DID as a string, or an empty string if resolution fails.
    /// </returns>
    Task<string> ResolveMention(string mention);
}

/// <summary>
/// Implementation of <see cref="IMentionResolver"/> that resolves Bluesky mentions to DIDs 
/// by calling the Bluesky API.
/// </summary>
public class MentionResolver : IMentionResolver
{
    private readonly Uri _baseUrl;
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MentionResolver"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="baseUrl">Base URL for the Bluesky API.</param>
    /// <param name="logger">Logger for recording diagnostic information.</param>
    [PublicAPI]
    public MentionResolver(IHttpClientFactory httpClientFactory, Uri baseUrl, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _baseUrl = baseUrl;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> ResolveMention(string mention)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var url = $"{_baseUrl.ToString().TrimEnd('/')}/xrpc/com.atproto.identity.resolveHandle";

        var requestUri = $"{url}?handle={mention.Replace("@", string.Empty)}";

        var response = await httpClient.GetAsync(requestUri);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error: {ResponseContent}", responseContent);

            // Return null if unable to resolve
            return string.Empty;
        }

        var json = JObject.Parse(responseContent);

        return json["did"]?.ToString() ?? string.Empty;
    }
}