using Newtonsoft.Json;

namespace X.Bluesky.Models.API;

internal interface IEmbed
{
    /// <summary>
    /// Embed type
    /// </summary>
    [JsonProperty("$type")]
    string Type { get; }
}