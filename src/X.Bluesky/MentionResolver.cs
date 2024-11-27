using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;

namespace X.Bluesky;

public interface IMentionResolver
{
    /// <summary>
    /// Resolve mention to DID format
    /// </summary>
    /// <param name="mention"></param>
    /// <returns></returns>
    Task<string> ResolveMention(string mention);
}

/// <summary>
/// Resolve mention to DID format
/// </summary>
public class MentionResolver : IMentionResolver
{
    private readonly Uri _baseUrl;
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Create a new instance of MentionResolver
    /// </summary>
    /// <param name="httpClientFactory"></param>
    /// <param name="baseUrl">Bluesky base url</param>
    /// <param name="logger"></param>
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