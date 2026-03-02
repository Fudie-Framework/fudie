namespace Fudie.Http;

/// <summary>
/// Custom problem details DTO used by <see cref="GlobalExceptionHandler"/>
/// and the Security authorization middleware to return RFC 7807 responses.
/// </summary>
public class CustomProblemDetails
{
    /// <summary>Gets or sets the URI reference that identifies the problem type.</summary>
    public string Type { get; set; } = "about:blank";

    /// <summary>Gets or sets a short, human-readable summary of the problem.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the HTTP status code.</summary>
    public int Status { get; set; }

    /// <summary>Gets or sets a human-readable explanation specific to this occurrence.</summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>Gets or sets the URI reference that identifies the specific occurrence.</summary>
    public string Instance { get; set; } = string.Empty;

    /// <summary>Gets or sets additional problem-specific properties.</summary>
    public Dictionary<string, object>? Extensions { get; set; }
}
