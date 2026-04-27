namespace IdeaForge.Agents.Abstractions;

/// <summary>
/// Type of person represented in the company directory.
/// </summary>
public enum PersonKind
{
    /// <summary>
    /// Person type has not been assigned.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Human person.
    /// </summary>
    Human = 1,

    /// <summary>
    /// AI person represented by an agent.
    /// </summary>
    Ai = 2
}
