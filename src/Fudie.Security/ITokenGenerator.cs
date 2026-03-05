namespace Fudie.Security;


/// <summary>
/// Generates signed JWT tokens for authenticated sessions.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates a signed JWT for a user session (cookie-based).
    /// Includes the <c>sid</c> claim with the session identifier.
    /// </summary>
    /// <param name="data">The session data to encode in the token.</param>
    /// <param name="sessionId">The session identifier to include as <c>sid</c> claim.</param>
    /// <returns>The signed JWT string.</returns>
    string GenerateUserToken(FudieTokenContext data, Guid sessionId);

    /// <summary>
    /// Generates a signed JWT for an external application (API key-based).
    /// Includes the <c>app</c> claim with the application identifier.
    /// </summary>
    /// <param name="data">The session data to encode in the token.</param>
    /// <param name="appId">The external application identifier to include as <c>app</c> claim.</param>
    /// <returns>The signed JWT string.</returns>
    string GenerateAppToken(FudieTokenContext data, Guid appId);
}