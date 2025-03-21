using System.Net;

namespace X.Bluesky;

/// <summary>
/// A simple implementation of <see cref="IHttpClientFactory"/> that creates and reuses 
/// a single <see cref="HttpClient"/> instance configured for Bluesky API requests.
/// </summary>
internal class BlueskyHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueskyHttpClientFactory"/> class.
    /// Creates a configured HttpClient with GZip and Deflate decompression enabled.
    /// </summary>
    public BlueskyHttpClientFactory()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _client = new HttpClient(handler);
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> instance to be used for Bluesky API requests.
    /// </summary>
    /// <param name="name">
    /// The name of the client. This parameter is ignored as this factory returns the same instance for all requests.
    /// </param>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    public HttpClient CreateClient(string name) => _client;
}