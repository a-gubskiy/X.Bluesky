using Microsoft.Extensions.Logging;

namespace X.Bluesky;

public class FileTypeHelper
{
    private readonly ILogger _logger;

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
                return uri.AbsolutePath.Substring(lastIndex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error extracting file extension: {ex.Message}", ex);
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