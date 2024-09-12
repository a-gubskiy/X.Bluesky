using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using X.Bluesky.Models;

namespace X.Bluesky;

public class EmbedCardBuilder
{
    private readonly ILogger _logger;
    private readonly FileTypeHelper _fileTypeHelper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Session _session;

    public EmbedCardBuilder(IHttpClientFactory httpClientFactory, Session session, ILogger logger)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _session = session;
        _fileTypeHelper = new FileTypeHelper(logger);
    }
    
    /// <summary>
    /// Create embed card
    /// </summary>
    /// <param name="url"></param>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public async Task<EmbedCard> Create(Uri url)
    {
        var extractor = new Web.MetaExtractor.Extractor();
        var metadata = await extractor.ExtractAsync(url);

        var card = new EmbedCard
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
                    card.Thumb = await UploadImageAndSetThumbAsync(new Uri(url, imgUrl));
                }
                else
                {
                    card.Thumb = await UploadImageAndSetThumbAsync(new Uri(imgUrl));    
                }
                
                _logger.LogInformation("EmbedCard created");
            }
        }

        return card;
    }

    private async Task<Thumb?> UploadImageAndSetThumbAsync(Uri imageUrl)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var imgResp = await httpClient.GetAsync(imageUrl);
        imgResp.EnsureSuccessStatusCode();

        var mimeType = _fileTypeHelper.GetMimeTypeFromUrl(imageUrl);

        var imageContent = new StreamContent(await imgResp.Content.ReadAsStreamAsync());
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