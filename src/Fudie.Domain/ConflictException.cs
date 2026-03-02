namespace Fudie.Domain;

/// <summary>
/// Exception thrown when a domain operation encounters a conflict (e.g. duplicate resource).
/// </summary>
/// <param name="message">A message describing the conflict.</param>
public class ConflictException(string message) : Exception(message);