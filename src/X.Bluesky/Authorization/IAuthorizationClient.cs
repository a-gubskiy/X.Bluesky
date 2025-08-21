using X.Bluesky.Models;

namespace X.Bluesky.Authorization;

/// <summary>
/// Provides an abstraction for acquiring an authenticated Bluesky AT Protocol session.
/// </summary>
public interface IAuthorizationClient
{
    /// <summary>
    /// Requests or retrieves an authenticated session with the underlying AT Protocol service.
    /// </summary>
    /// <returns>A task that resolves to an authenticated <see cref="Session"/>.</returns>
    /// <exception cref="System.Net.Http.HttpRequestException">
    /// May be thrown by implementations if an HTTP call fails or returns a non\-success status code.
    /// </exception>
    /// <exception cref="Newtonsoft.Json.JsonException">
    /// May be thrown by implementations if response payload deserialization fails.
    /// </exception>
    Task<Session> GetSession();
}