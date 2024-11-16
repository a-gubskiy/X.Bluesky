using Microsoft.Extensions.Logging;

namespace X.Bluesky;

public class FileTypeHelper
{
    private readonly ILogger _logger;

    /// <summary>
    /// Create a new instance of FileTypeHelper
    /// </summary>
    /// <param name="logger"></param>
    public FileTypeHelper(ILogger logger)
    {
        _logger = logger;
    }
    
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

    public string GetMimeTypeFromUrl(string url)
    {
        var extension = GetFileExtensionFromUrl(url);
        
        return GetMimeType(extension);
    }
    
    public string GetMimeTypeFromUrl(Uri uri)
    {
        return GetMimeTypeFromUrl(uri.ToString());
    }
    
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