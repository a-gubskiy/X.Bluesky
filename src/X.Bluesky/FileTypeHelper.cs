using Microsoft.Extensions.Logging;

namespace X.Bluesky;

/// <summary>
/// Helper class for determining file types and MIME types from URLs and file extensions.
/// </summary>
internal class FileTypeHelper
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTypeHelper"/> class.
    /// </summary>
    /// <param name="logger">The logger used for recording diagnostic information.</param>
    public FileTypeHelper(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts the file extension from a URL.
    /// </summary>
    /// <param name="url">The URL to extract the file extension from.</param>
    /// <returns>The file extension (including the dot) if found; otherwise, an empty string.</returns>
    public string GetFileExtensionFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var lastIndex = uri.AbsolutePath.LastIndexOf('.');

            if (lastIndex != -1 && lastIndex < uri.AbsolutePath.Length - 1)
            {
                // Return the file extension, including the dot
                return uri.AbsolutePath[lastIndex..];
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting file extension: {Message}", ex.Message);
        }

        // Return an empty string if no extension found or an error occurred
        return string.Empty;
    }

    /// <summary>
    /// Determines the MIME type of a resource based on its URL.
    /// </summary>
    /// <param name="url">The URL of the resource.</param>
    /// <returns>The MIME type as a string, or "application/octet-stream" if the type cannot be determined.</returns>
    public string GetMimeTypeFromUrl(string url)
    {
        var extension = GetFileExtensionFromUrl(url);

        return GetMimeType(extension);
    }

    /// <summary>
    /// Determines the MIME type of a resource based on its URI.
    /// </summary>
    /// <param name="uri">The URI of the resource.</param>
    /// <returns>The MIME type as a string, or "application/octet-stream" if the type cannot be determined.</returns>
    public string GetMimeTypeFromUrl(Uri uri)
    {
        return GetMimeTypeFromUrl(uri.ToString());
    }

    /// <summary>
    /// Determines the MIME type based on a file extension.
    /// </summary>
    /// <param name="extension">The file extension (with or without a leading dot).</param>
    /// <returns>
    /// The MIME type corresponding to the extension, or "application/octet-stream" 
    /// if the extension is empty, null, or not recognized.
    /// </returns>
    public string GetMimeType(string extension)
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

        return extension.ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".tiff" or ".tif" => "image/tiff",
            ".avif" => "image/avif",
            ".heic" => "image/heic",
            ".bmp" => "image/bmp",
            ".ico" or ".icon" => "image/x-icon",
            _ => "application/octet-stream"
        };
    }
}