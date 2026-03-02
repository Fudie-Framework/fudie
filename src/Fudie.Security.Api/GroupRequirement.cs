namespace Fudie.Security.Api;

/// <summary>
/// Metadata marker that requires the user to belong to a specific permission group.
/// </summary>
public class GroupRequirement(string group, string description)
{
    /// <summary>Required permission group name.</summary>
    public string Group { get; } = group;

    /// <summary>Human-readable description of the group requirement.</summary>
    public string Description { get; } = description;
}
