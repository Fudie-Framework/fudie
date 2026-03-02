namespace Fudie.Features;

/// <summary>
/// Provides metadata about an aggregate: display name, icon, and descriptions.
/// </summary>
public interface IAggregateDescription
{
    /// <summary>Gets the aggregate identifier.</summary>
    string Id { get; }

    /// <summary>Gets the human-readable display name.</summary>
    string DisplayName { get; }

    /// <summary>Gets an optional icon identifier.</summary>
    string? Icon { get; }

    /// <summary>Gets the description shown for read operations.</summary>
    string ReadDescription { get; }

    /// <summary>Gets the description shown for write operations.</summary>
    string WriteDescription { get; }
}
