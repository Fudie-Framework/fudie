namespace Fudie.Domain;

/// <summary>
/// Exception thrown when a domain operation encounters a conflict (e.g. duplicate resource).
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Gets the optional error code identifying the specific conflict rule.
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance with the specified message.
    /// </summary>
    /// <param name="message">A message describing the conflict.</param>
    public ConflictException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance with the specified message and error code.
    /// </summary>
    /// <param name="message">A message describing the conflict.</param>
    /// <param name="errorCode">The error code identifying the conflict rule.</param>
    public ConflictException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }
}