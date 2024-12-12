using Newtonsoft.Json;

namespace X.Bluesky.Models;

public interface IEmbed
{
    /// <summary>
    /// Embed type
    /// </summary>
    [JsonProperty("$type")]
    string Type { get; }
}