using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization; // Make sure to include Newtonsoft.Json package

namespace X.Bluesky;

[PublicAPI]
public interface IBlueskyClient
{
    Task Post(string text, Uri? uri);
    Task Post(string text);
}

public class BlueskyClient : IBlueskyClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _identifier;
    private readonly string _appPassword;
    private readonly IReadOnlyCollection<string> _languages;

    public BlueskyClient(IHttpClientFactory httpClientFactory, string identifier, string appPassword)
    {
        _httpClientFactory = httpClientFactory;
        _identifier = identifier;
        _appPassword = appPassword;
        _languages = new List<string> { "en", "en-US" };
    }

    public BlueskyClient(string identifier, string appPassword)
        : this(new HttpClientFactory(), identifier, appPassword)
    {
    }

    private async Task<bool> CreatePost(BlueskySession blueskySession, string text, Uri? url)
    {
        // Fetch the current time in ISO 8601 format, with "Z" to denote UTC
        var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        // Required fields for the post
        var post = new BlueskyPost
        {
            Type = "app.bsky.feed.post",
            //Text = $"{text} {url}".Trim(),
            Text = text,
            CreatedAt = now,
            Langs = _languages.ToList()
        };

        if (url != null)
        {
            // var byteStart = text.Length + 1;
            // var byteEnd = byteStart + url.ToString().Length;

            // post.Facets = new List<BlueskyFacet>
            // {
            //     new()
            //     {
            //         Index = new BlueskyFacetIndex
            //         {
            //             ByteStart = byteStart,
            //             ByteEnd = byteEnd
            //         },
            //         Features =
            //         [
            //             new BlueskyFacetFeature
            //             {
            //                 Type = "app.bsky.richtext.facet#link",
            //                 Uri = url
            //             }
            //         ]
            //     }
            // };

            post.Embed = new BlueskyEmbed
            {
                External = await CreateEmbedCardAsync(url, blueskySession.AccessJwt),
                Type = "app.bsky.embed.external"
            };
        }

        var requestUri = "https://bsky.social/xrpc/com.atproto.repo.createRecord";

        var requestData = new BlueskyPostRequest
        {
            Repo = blueskySession.Did,
            Collection = "app.bsky.feed.post",
            Record = post
        };

        var jsonRequest = JsonConvert.SerializeObject(requestData, Formatting.Indented, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        });

        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();

        // Add the Authorization header with the bearer token
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", blueskySession.AccessJwt);

        var response = await httpClient.PostAsync(requestUri, content);

        var ensureSuccessStatusCode = response.EnsureSuccessStatusCode();

        // This throws an exception if the HTTP response status is an error code.
        return ensureSuccessStatusCode.IsSuccessStatusCode;
    }

    public async Task Post(string text, Uri? uri)
    {
        var session = await Authorize(_identifier, _appPassword);

        if (session == null)
        {
            throw new AuthenticationException();
        }

        await CreatePost(session, text, uri);
    }

    public Task Post(string text) => Post(text, null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="identifier">Account identifier</param>
    /// <param name="password">App password</param>
    /// <returns></returns>
    private async Task<BlueskySession?> Authorize(string identifier, string password)
    {
        var requestData = new
        {
            identifier = identifier,
            password = password
        };

        var json = JsonConvert.SerializeObject(requestData);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient();

        var uri = "https://bsky.social/xrpc/com.atproto.server.createSession";
        var response = await httpClient.PostAsync(uri, content);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<BlueskySession>(jsonResponse);
    }

    private async Task<BlueskyEmbedCard> CreateEmbedCardAsync(Uri url, string accessToken)
    {
        var extractor = new Web.MetaExtractor.Extractor();
        var metadata = await extractor.ExtractAsync(url);

        var card = new BlueskyEmbedCard
        {
            Uri = url.ToString(),
            Title = metadata.Title,
            Description = metadata.Description
        };

        if (metadata.Images != null && metadata.Images.Any())
        {
            var imgUrl = metadata.Images.FirstOrDefault();

            if (imgUrl != null)
            {
                if (!imgUrl.Contains("://"))
                {
                    imgUrl = new Uri(url, imgUrl).ToString();
                }

                card.Thumb = await UploadImageAndSetThumbAsync(imgUrl, accessToken);
            }
        }

        return card;
    }

    // private async Task<string> UploadImageAndSetThumbAsync(string imageUrl, string accessToken)
    // {
    //     var httpClient = _httpClientFactory.CreateClient();
    //
    //     var imgResp = await httpClient.GetAsync(imageUrl);
    //     imgResp.EnsureSuccessStatusCode();
    //     
    //     var extension = GetFileExtensionFromUrl(imageUrl);
    //     var mimeType = GetMimeType(extension);
    //     
    //     
    //     var imageContent = new StreamContent(await imgResp.Content.ReadAsStreamAsync());
    //     
    //     imageContent.Headers.Add("Authorization", "Bearer " + accessToken);
    //     imageContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
    //     
    //
    //     var uri = "https://bsky.social/xrpc/com.atproto.repo.uploadBlob";
    //
    //     var blobResp = await httpClient.PostAsync(uri, imageContent);
    //
    //     blobResp.EnsureSuccessStatusCode();
    //
    //     var blobRespContent = await blobResp.Content.ReadAsStringAsync();
    //     var blobJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(blobRespContent);
    //
    //     if (blobJson != null && blobJson.TryGetValue("blob", out var blob))
    //     {
    //         return blob;
    //     }
    //
    //     return string.Empty;
    // }
    
    private async Task<BlueskyThumb> UploadImageAndSetThumbAsync(string imageUrl, string accessToken)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var imgResp = await httpClient.GetAsync(imageUrl);
        imgResp.EnsureSuccessStatusCode();
    
        var extension = GetFileExtensionFromUrl(imageUrl);
        var mimeType = GetMimeType(extension);
    
        var imageContent = new StreamContent(await imgResp.Content.ReadAsStreamAsync());
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
    
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://bsky.social/xrpc/com.atproto.repo.uploadBlob")
        {
            Content = imageContent,
        };

        // Add the Authorization header with the access token to the request message
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var blobResp = await httpClient.SendAsync(requestMessage);

        blobResp.EnsureSuccessStatusCode();

        var blobRespContent = await blobResp.Content.ReadAsStringAsync();
        var blob = JsonConvert.DeserializeObject<BlueskyBlobResponse>(blobRespContent);

        var card = blob.Blob;
        card.Type = "blob"; //fix it

        return card;
    }

    public static string GetFileExtensionFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            // Extract the last segment of the URL path
            string path = uri.AbsolutePath;
            int lastIndex = path.LastIndexOf('.');

            if (lastIndex != -1 && lastIndex < path.Length - 1)
            {
                // Return the file extension, including the dot
                return path.Substring(lastIndex);
            }
        }
        catch (Exception ex)
        {
            // Handle or log the exception as needed
            Console.WriteLine($"Error extracting file extension: {ex.Message}");
        }

        // Return an empty string or null if no extension found or an error occurred
        return "";
    }


    public static string GetMimeType(string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return "application/octet-stream";
        }

        // Ensure the extension starts with a dot
        if (extension[0] != '.')
        {
            extension = "." + extension;
        }

        switch (extension.ToLower())
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".gif":
                return "image/gif";
            case ".webp":
                return "image/webp";
            case ".svg":
                return "image/svg+xml";
            case ".tiff":
            case ".tif":
                return "image/tiff";
            case ".avif":
                return "image/avif";
            case ".heic":
                return "image/heic";
            case ".bmp":
                return "image/bmp";
            case ".ico":
            case ".icon":
                return "image/x-icon";
            default:
                return "application/octet-stream"; // Default MIME type
        }
    }
}