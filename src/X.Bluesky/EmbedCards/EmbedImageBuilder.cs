using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using X.Bluesky.Models;

namespace X.Bluesky.EmbedCards;

public class EmbedImageBuilder : EmbedBuilder
{
    private readonly Uri _baseUrl;
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public EmbedImageBuilder(IHttpClientFactory httpClientFactory, Session session, Uri baseUrl, ILogger logger)
        : base(session)
    {
        _baseUrl = baseUrl;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }
    
    /// <summary>
    /// Upload image to the Bsky server
    /// </summary>
    /// <param name="image">
    /// Image content
    /// </param>
    /// <param name="mimeType"></param>
    /// <returns></returns>
    public async Task<Thumb?> UploadImage(byte[] image, string mimeType)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var imageContent = new StreamContent(new MemoryStream(image));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

        var requestUri = $"{_baseUrl.ToString().TrimEnd('/')}/xrpc/com.atproto.repo.uploadBlob";
        
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = imageContent,
        };

        // Add the Authorization header with the access token to the request message
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessJwt);

        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var blob = JsonConvert.DeserializeObject<BlobResponse>(json);

        var card = blob?.Blob;

        if (card != null)
        {
            // ToDo: fix it
            // This is hack for fix problem when Type is empty after deserialization
            card.Type = "blob";
        }

        return card;
    }
}