using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using X.Bluesky.Models;
using X.Bluesky.Models.API;

namespace X.Bluesky.EmbedCards;

internal class EmbedImageBuilder : EmbedBuilder
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
    /// <param name="images"></param>
    /// <returns></returns>
    internal async Task<IEmbed> GetEmbedCard(IEnumerable<Image> images)
    {
        var embed = new EmbedImage();

        foreach (var image in images)
        {
            var thumb = await UploadImage(image.Content, image.MimeType);

            embed.Images.Add(new ImageData
            {
                Image = thumb,
                Alt = image.Alt,
                // AspectRatio = null
            });
        }

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
    internal async Task<Thumb> UploadImage(byte[] image, string mimeType)
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