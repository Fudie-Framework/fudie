namespace Fudie.Domain;

/// <summary>
/// Exception thrown when a domain operation is not authorized.
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Gets the optional error code identifying the specific authorization rule.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">A message describing the authorization failure.</param>
    public UnauthorizedException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and error code.
    /// </summary>
    /// <param name="message">A message describing the authorization failure.</param>
    /// <param name="errorCode">The error code identifying the authorization rule.</param>
    public UnauthorizedException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}
