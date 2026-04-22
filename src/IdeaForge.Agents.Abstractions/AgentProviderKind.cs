namespace IdeaForge.Agents.Abstractions;

/// <summary>
/// Supported LLM provider types used by employee-style agents.
/// </summary>
public enum AgentProviderKind
{
    /// <summary>
    /// Unspecified provider.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// GitHub Copilot based provider.
    /// </summary>
    Copilot = 1,

    /// <summary>
    /// OpenAI Codex CLI based provider.
    /// </summary>
    Codex = 2
}
