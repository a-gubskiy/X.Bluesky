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
    /// 
    /// </summary>
    /// <param name="image"></param>
    /// <param name="mimeType"></param>
    /// <param name="alt"></param>
    /// <returns></returns>
    public async Task<IEmbed> GetEmbedCard(byte[] image, string mimeType, string alt)
    {
        var thumb = await UploadImage(image, mimeType);
        
        var embed = new EmbedImage
        {
            Images =
            [
                new()
                {
                    Image = thumb,
                    // AspectRatio = new AspectRatio
                    // {
                    //     Width = 1,
                    //     Height = 1
                    // },
                    Alt = alt
                }
            ]
        };

        return embed;
    }
    
    /// <summary>
    /// Upload image to the Bsky server
    /// </summary>
    /// <param name="image">
    /// Image content
    /// </param>
    /// <param name="mimeType"></param>
    /// <returns></returns>
    public async Task<Thumb> UploadImage(byte[] image, string mimeType)
    {
        if (image.Length == 0)
        {
            _logger.LogError("Image content is empty");
            
            throw new ArgumentException("Image content is empty", nameof(image));
        }
        
        if (image.Length > 1000000)
        {
            _logger.LogError($"image file size too large. 1000000 bytes maximum, got: {image.Length}");
            
            throw new Exception($"image file size too large. 1000000 bytes maximum, got: {image.Length}");
        }
        
        var httpClient = _httpClientFactory.CreateClient();

        var imageContent = new StreamContent(new MemoryStream(image));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

        var requestUri = $"{_baseUrl.ToString().TrimEnd('/')}/xrpc/com.atproto.repo.uploadBlob";
        
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = imageContent,
        };

        // Add the Authorization header with the access token to the request message
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.AccessJwt);

        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var blob = JsonConvert.DeserializeObject<BlobResponse>(json);

        if (blob == null || blob.Blob == null)
        {
            _logger.LogError("Failed to upload image");
            
            throw new Exception("Failed to upload image");
        }

        var card = blob.Blob;
        
        // ToDo: fix it
        // This is hack for fix problem when Type is empty after deserialization
        card.Type = "blob";

        return card;
    }
}