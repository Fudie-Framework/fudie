namespace Fudie.Security.Http;

/// <summary>
/// Describes an aggregate for catalog registration and scope resolution.
/// </summary>
public interface IAggregateDescription
{
    /// <summary>Unique aggregate identifier used for scope prefixes.</summary>
    string Id { get; }

    /// <summary>Human-readable display name.</summary>
    string DisplayName { get; }

    /// <summary>Optional icon identifier.</summary>
    string? Icon { get; }

    /// <summary>Description for read-scope operations.</summary>
    string ReadDescription { get; }

    /// <summary>Description for write-scope operations.</summary>
    string WriteDescription { get; }
}
