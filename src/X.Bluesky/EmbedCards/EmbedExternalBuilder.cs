using Microsoft.Extensions.Logging;
using X.Bluesky.Models;
using X.Bluesky.Models.API;
using X.Web.MetaExtractor;
using X.Web.MetaExtractor.ContentLoaders.HttpClient;
using X.Web.MetaExtractor.LanguageDetectors;

namespace X.Bluesky.EmbedCards;

internal class EmbedExternalBuilder : EmbedBuilder
{
    private readonly ILogger _logger;
    private readonly FileTypeHelper _fileTypeHelper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EmbedImageBuilder _embedImageBuilder;
    private readonly IExtractor _extractor;

    public EmbedExternalBuilder(IHttpClientFactory httpClientFactory, Session session, Uri baseUrl, ILogger logger)
        : this(
            httpClientFactory,
            new Extractor("", new HttpClientPageContentLoader(httpClientFactory), new LanguageDetector()),
            session,
            baseUrl,
            logger)
    {
    }

    internal EmbedExternalBuilder(
        IHttpClientFactory httpClientFactory,
        IExtractor extractor,
        Session session,
        Uri baseUrl,
        ILogger logger)
        : base(session)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _fileTypeHelper = new FileTypeHelper(logger);
        _extractor = extractor;
        _embedImageBuilder = new EmbedImageBuilder(httpClientFactory, session, baseUrl, logger);
    }

    /// <summary>
    /// Create embed card
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    internal async Task<IEmbed> GetEmbedCard(Uri url)
    {
        var metadata = await _extractor.ExtractAsync(url);

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

        return new EmbedExternal
        {
            External = card
        };
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

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var mimeType = _fileTypeHelper.GetMimeTypeFromUrl(url);

        var image = await response.Content.ReadAsByteArrayAsync();

        var thumb = await _embedImageBuilder.UploadImage(image, mimeType);

        return thumb;
    }
}