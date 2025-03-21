using System.Net;

namespace X.Bluesky;

internal class BlueskyHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    public BlueskyHttpClientFactory()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _client = new HttpClient(handler);
    }

    public HttpClient CreateClient(string name) => _client;
}