namespace X.Bluesky.Models;

/// <summary>
/// Represents an authentication session for the Bluesky API.
/// Contains the necessary credentials used to authenticate requests.
/// </summary>
public record Session
{
    /// <summary>
    /// Gets or initializes the JSON Web Token (JWT) used for authenticating API requests.
    /// This token should be included in the Authorization header of HTTP requests.
    /// </summary>
    /// <value>A string containing the access JWT. Defaults to an empty string.</value>
    public string AccessJwt { get; init; } = "";

    /// <summary>
    /// Gets or initializes the Decentralized Identifier (DID) for the authenticated user.
    /// This serves as the unique identifier for the user within the Bluesky network.
    /// </summary>
    /// <value>A string containing the user's DID. Defaults to an empty string.</value>
    public string Did { get; init; } = "";
}