using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using X.Bluesky.Models;

namespace X.Bluesky.EmbedCards;

public class EmbedExternalBuilder : EmbedBuilder
{
    private readonly ILogger _logger;
    private readonly FileTypeHelper _fileTypeHelper;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmbedExternalBuilder(IHttpClientFactory httpClientFactory, Session session, ILogger logger) 
        : base(session)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _fileTypeHelper = new FileTypeHelper(logger);
    }

    /// <summary>
    /// Create embed card
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public async Task<IEmbed> GetEmbedCard(Uri url)
    {
        var extractor = new Web.MetaExtractor.Extractor();
        var metadata = await extractor.ExtractAsync(url);

        var card = new External
        {
            Uri = url.ToString(),
            Title = metadata.Title,
            Description = metadata.Description
        };

        if (metadata.Images.Any())
        {
            var imgUrl = metadata.Images.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(imgUrl))
            {
                if (!imgUrl.Contains("://"))
                {
                    card.Thumb = await UploadImage(new Uri(url, imgUrl));
                }
                else
                {
                    card.Thumb = await UploadImage(new Uri(imgUrl));
                }

                _logger.LogInformation("EmbedCard created");
            }
        }

        var embed = new EmbedExternal
        {
            External = card
        };

        return embed;
    }

    /// <summary>
    /// Upload image to the Bsky server
    /// </summary>
    /// <param name="url">
    /// Image URL
    /// </param>
    /// <returns></returns>
    private async Task<Thumb?> UploadImage(Uri url)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var imgResp = await httpClient.GetAsync(url);
        imgResp.EnsureSuccessStatusCode();

        var mimeType = _fileTypeHelper.GetMimeTypeFromUrl(url);


        var image = await imgResp.Content.ReadAsByteArrayAsync();
        
        return await UploadImage(image, mimeType);
    }

    /// <summary>
    /// Upload image to the Bsky server
    /// </summary>
    /// <param name="image">
    /// Image content
    /// </param>
    /// <param name="mimeType"></param>
    /// <returns></returns>
    private async Task<Thumb?> UploadImage(byte[] image, string mimeType)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var imageContent = new StreamContent(new MemoryStream(image));
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://bsky.social/xrpc/com.atproto.repo.uploadBlob")
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