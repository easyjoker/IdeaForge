namespace IdeaForge.Agents.Abstractions;

/// <summary>
/// Lifecycle status of an employee-style agent.
/// </summary>
public enum AgentStatus
{
    /// <summary>
    /// Draft configuration that is not yet ready for normal use.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Active employee that can accept work.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Temporarily disabled employee that should not accept work.
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// Archived employee kept for history only.
    /// </summary>
    Archived = 3
}
