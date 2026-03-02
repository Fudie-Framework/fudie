namespace Fudie.Security;


/// <summary>
/// Generates signed JWT tokens for authenticated sessions.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates a signed JWT containing the session claims.
    /// </summary>
    /// <param name="data">The session data to encode in the token.</param>
    /// <returns>The signed JWT string.</returns>
    string GenerateSessionToken(FudieTokenContext data);
}