using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;

namespace X.Bluesky;

public interface IMentionResolver
{
    Task<string> ResolveMention(string mention);
}

/// <summary>
/// Resolve mention to DID format
/// </summary>
public class MentionResolver : IMentionResolver
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    
    public MentionResolver()
        : this(new BlueskyHttpClientFactory(), new NullLogger<MentionResolver>())
    {
    }
    
    public MentionResolver(IHttpClientFactory httpClientFactory)
        : this(httpClientFactory, new NullLogger<MentionResolver>())
    {
    }

    public MentionResolver(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public MentionResolver(IHttpClientFactory httpClientFactory, ILogger<MentionResolver> logger)
        : this(httpClientFactory, (ILogger)logger)
    {
    }

    public async Task<string> ResolveMention(string mention)
    {
        var httpClient = _httpClientFactory.CreateClient();

        const string url = "https://bsky.social/xrpc/com.atproto.identity.resolveHandle";
        
        var requestUri = $"{url}?handle={mention.Replace("@", string.Empty)}";
        
        var response = await httpClient.GetAsync(requestUri);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var json = JObject.Parse(responseContent);

            return json["did"]?.ToString() ?? string.Empty;
        }
        else
        {
            _logger.LogError(responseContent);
        }

        // Return null if unable to resolve
        return string.Empty;
    }
}