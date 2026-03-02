namespace Fudie.Domain;

/// <summary>
/// Exception thrown when a domain operation is not authorized.
/// </summary>
/// <param name="message">A message describing the authorization failure.</param>
public class UnauthorizedException(string message) : Exception(message);
