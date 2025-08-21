using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using X.Bluesky.Models;
using X.Bluesky.Models.API;

namespace X.Bluesky.EmbedCards;

/// <summary>
/// Builds image embed cards for AT Protocol posts by uploading image blobs and composing
/// an <see cref="X.Bluesky.Models.API.EmbedImage"/> payload.
/// </summary>
/// <remarks>
/// Uses <see cref="System.Net.Http.IHttpClientFactory"/> and a bearer token from <see cref="X.Bluesky.Models.Session"/>
/// to POST blobs to the Bluesky repository API, and assembles <see cref="X.Bluesky.Models.API.ImageData"/> entries.
/// See <c>GetEmbedCard</c> to construct an <see cref="X.Bluesky.Models.API.IEmbed"/> and <c>UploadImage</c> for the upload logic.
/// </remarks>
/// <seealso cref="EmbedBuilder"/>
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
    /// Builds an embed card for a collection of images by uploading each image and
    /// assembling the corresponding <see cref="ImageData"/> entries.
    /// </summary>
    /// <param name="images">
    /// A non\-null sequence of <see cref="Image"/> items to embed. Each item must contain
    /// binary <c>Content</c> and a valid <c>MimeType</c>. Optional fields include <c>Alt</c>,
    /// <c>Width</c>, and <c>Height</c>.
    /// </param>
    /// <returns>
    /// An awaitable task producing an <see cref="IEmbed"/> (specifically an <see cref="EmbedImage"/>)
    /// that contains all successfully uploaded images.
    /// </returns>
    /// <remarks>
    /// For each image, the method uploads its content via <c>UploadImage</c> and adds the returned
    /// <see cref="Thumb"/> along with metadata. Aspect ratio is set only when both width and height are provided.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Propagated from <c>UploadImage</c> when the image content is empty.
    /// </exception>
    /// <exception cref="Exception">
    /// Propagated from <c>UploadImage</c> when the file size exceeds the allowed limit or the upload fails.
    /// </exception>
    internal async Task<IEmbed> GetEmbedCard(IEnumerable<Image> images)
    {
        // Container for the resulting image embed payload.
        var embed = new EmbedImage();
    
        // Upload each image and add its metadata to the embed.
        foreach (var image in images)
        {
            var thumb = await UploadImage(image.Content, image.MimeType);
    
            embed.Images.Add(new ImageData
            {
                Image = thumb,
                Alt = image.Alt,
                // Only set aspect ratio when both dimensions are available.
                AspectRatio = image.Width is not null && image.Height is not null
                    ? new AspectRatio() { Width = image.Width.Value, Height = image.Height.Value }
                    : null
            });
        }
    
        return embed;
    }

    /// <summary>
    /// Uploads an image blob to the AT Protocol repository and returns its <see cref="Thumb"/> descriptor.
    /// </summary>
    /// <param name="image">
    /// Raw image bytes to upload. Must be non-empty and less than or equal to 1,000,000 bytes.
    /// </param>
    /// <param name="mimeType">
    /// The image MIME type (e.g., "image/png", "image/jpeg") used for the request's Content-Type header.
    /// </param>
    /// <returns>
    /// A task that resolves to the uploaded image <see cref="Thumb"/>.
    /// </returns>
    /// <remarks>
    /// Sends a POST request to '/xrpc/com.atproto.repo.uploadBlob' with a bearer token from <see cref="Session.AccessJwt"/>.
    /// After deserialization, forces the blob <c>Type</c> to "blob" to work around an empty type issue.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="image"/> is empty.
    /// </exception>
    /// <exception cref="Exception">
    /// Thrown when the file size exceeds 1,000,000 bytes or when the upload fails. HTTP errors from
    /// <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/> are also propagated.
    /// </exception>
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
        // This is a hack to fix the issue when Type is empty after deserialization.
        card.Type = "blob";
    
        return card;
    }
}
